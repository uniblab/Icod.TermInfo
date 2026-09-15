/*
 * HDB00 research probe for ncurses hashed terminfo databases.
 *
 * This is development evidence, not production Icod.TermInfo code. It uses the
 * Berkeley DB C API to mirror the bounded lookup shape in ncurses read_entry.c:
 * an exact terminal-name key may lead through an index record (marker 2) to a
 * compiled-data record (marker 0). The bytes after marker 0 are written without
 * interpretation so Icod.TermInfo's existing CompiledTermInfoParser can remain
 * authoritative.
 */

#include <db.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define HDB00_DATA_MARKER 0u
#define HDB00_INDEX_MARKER 2u
#define HDB00_MAX_HOPS 3u

static void close_database(DB *database) {
    if (database != NULL) {
        int rc = database->close(database, 0);
        if (rc != 0) {
            fprintf(stderr, "Berkeley DB close failed: %s\n", db_strerror(rc));
        }
    }
}

static int write_payload(const char *path, const void *data, size_t size) {
    FILE *stream = fopen(path, "wb");
    if (stream == NULL) {
        fprintf(stderr, "Cannot open '%s' for output: %s\n", path, strerror(errno));
        return 1;
    }

    if (size != 0 && fwrite(data, 1, size, stream) != size) {
        fprintf(stderr, "Cannot write '%s': %s\n", path, strerror(errno));
        fclose(stream);
        return 1;
    }

    if (fclose(stream) != 0) {
        fprintf(stderr, "Cannot close '%s': %s\n", path, strerror(errno));
        return 1;
    }

    return 0;
}

static int count_records(DB *database, unsigned *data_count, unsigned *index_count) {
    DBC *cursor = NULL;
    DBT key;
    DBT value;
    int rc;

    *data_count = 0;
    *index_count = 0;

    rc = database->cursor(database, NULL, &cursor, 0);
    if (rc != 0) {
        fprintf(stderr, "Cannot create Berkeley DB cursor: %s\n", db_strerror(rc));
        return 1;
    }

    memset(&key, 0, sizeof(key));
    memset(&value, 0, sizeof(value));

    rc = cursor->get(cursor, &key, &value, DB_FIRST);
    while (rc == 0) {
        if (value.size == 0 || value.data == NULL) {
            fprintf(stderr, "Encountered an empty hashed-term record.\n");
            cursor->close(cursor);
            return 1;
        }

        switch (((const unsigned char *)value.data)[0]) {
            case HDB00_DATA_MARKER:
                (*data_count)++;
                break;
            case HDB00_INDEX_MARKER:
                (*index_count)++;
                break;
            default:
                fprintf(
                    stderr,
                    "Encountered unknown ncurses hashed-term record marker %u.\n",
                    (unsigned)((const unsigned char *)value.data)[0]
                );
                cursor->close(cursor);
                return 1;
        }

        memset(&key, 0, sizeof(key));
        memset(&value, 0, sizeof(value));
        rc = cursor->get(cursor, &key, &value, DB_NEXT);
    }

    if (rc != DB_NOTFOUND) {
        fprintf(stderr, "Berkeley DB cursor iteration failed: %s\n", db_strerror(rc));
        cursor->close(cursor);
        return 1;
    }

    rc = cursor->close(cursor);
    if (rc != 0) {
        fprintf(stderr, "Berkeley DB cursor close failed: %s\n", db_strerror(rc));
        return 1;
    }

    return 0;
}

int main(int argc, char **argv) {
    DB *database = NULL;
    DBT key;
    DBT value;
    char *owned_key = NULL;
    size_t key_size;
    unsigned hop;
    unsigned data_count;
    unsigned index_count;
    const char *version;
    int major;
    int minor;
    int patch;
    int rc;

    if (argc != 4) {
        fprintf(stderr, "Usage: %s DATABASE TERM OUTPUT\n", argv[0]);
        return 64;
    }

    version = db_version(&major, &minor, &patch);
    printf("Berkeley DB: %s (%d.%d.%d)\n", version, major, minor, patch);

    rc = db_create(&database, NULL, 0);
    if (rc != 0) {
        fprintf(stderr, "db_create failed: %s\n", db_strerror(rc));
        return 1;
    }

    rc = database->open(
        database,
        NULL,
        argv[1],
        NULL,
        DB_HASH,
        DB_RDONLY,
        0
    );
    if (rc != 0) {
        fprintf(stderr, "Cannot open '%s' as DB_HASH: %s\n", argv[1], db_strerror(rc));
        close_database(database);
        return 1;
    }

    key_size = strlen(argv[2]);
    owned_key = malloc(key_size == 0 ? 1 : key_size);
    if (owned_key == NULL) {
        fprintf(stderr, "Cannot allocate lookup key.\n");
        close_database(database);
        return 1;
    }
    if (key_size != 0) {
        memcpy(owned_key, argv[2], key_size);
    }

    for (hop = 0; hop < HDB00_MAX_HOPS; hop++) {
        unsigned char marker;

        memset(&key, 0, sizeof(key));
        key.data = owned_key;
        key.size = (u_int32_t)key_size;
        memset(&value, 0, sizeof(value));

        rc = database->get(database, NULL, &key, &value, 0);
        if (rc == DB_NOTFOUND) {
            fprintf(stderr, "Clean miss for '%s'.\n", argv[2]);
            free(owned_key);
            close_database(database);
            return 3;
        }
        if (rc != 0) {
            fprintf(stderr, "Berkeley DB get failed: %s\n", db_strerror(rc));
            free(owned_key);
            close_database(database);
            return 1;
        }
        if (value.size == 0 || value.data == NULL) {
            fprintf(stderr, "Lookup returned an empty ncurses record.\n");
            free(owned_key);
            close_database(database);
            return 1;
        }

        marker = ((const unsigned char *)value.data)[0];
        if (marker == HDB00_DATA_MARKER) {
            size_t payload_size = (size_t)value.size - 1u;
            const unsigned char *payload = (const unsigned char *)value.data + 1u;

            if (write_payload(argv[3], payload, payload_size) != 0) {
                free(owned_key);
                close_database(database);
                return 1;
            }

            if (count_records(database, &data_count, &index_count) != 0) {
                free(owned_key);
                close_database(database);
                return 1;
            }

            printf("Lookup: %s\n", argv[2]);
            printf("Hops: %u\n", hop + 1u);
            printf("Compiled bytes: %zu\n", payload_size);
            printf("Data records: %u\n", data_count);
            printf("Index records: %u\n", index_count);

            free(owned_key);
            close_database(database);
            return 0;
        }

        if (marker != HDB00_INDEX_MARKER || value.size <= 1u) {
            fprintf(stderr, "Unexpected ncurses hashed-term marker %u.\n", (unsigned)marker);
            free(owned_key);
            close_database(database);
            return 1;
        }

        key_size = (size_t)value.size - 1u;
        {
            char *next_key = malloc(key_size == 0 ? 1 : key_size);
            if (next_key == NULL) {
                fprintf(stderr, "Cannot allocate index target key.\n");
                free(owned_key);
                close_database(database);
                return 1;
            }
            memcpy(next_key, (const unsigned char *)value.data + 1u, key_size);
            free(owned_key);
            owned_key = next_key;
        }
    }

    fprintf(stderr, "Lookup exceeded the bounded ncurses index chain.\n");
    free(owned_key);
    close_database(database);
    return 1;
}
