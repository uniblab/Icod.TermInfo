/*
 * HDB07C CI-only Berkeley DB record repacker.
 *
 * This native helper is test infrastructure, not production Icod.TermInfo
 * code. It copies exact key/value bytes into a new Hash database whose
 * metadata byte order is selected before the destination is opened.
 */

#define _DEFAULT_SOURCE 1
#include <sys/types.h>

#include <db.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main(int argc, char **argv) {
    DB *source = NULL;
    DB *destination = NULL;
    DBC *cursor = NULL;
    DBT key;
    DBT value;
    unsigned long record_count = 0;
    int selected_order;
    int status;
    int result = 1;

    if (
        argc != 4
        || (strcmp(argv[3], "1234") != 0 && strcmp(argv[3], "4321") != 0)
    ) {
        fprintf(
            stderr,
            "Usage: %s SOURCE_DATABASE DESTINATION_DATABASE 1234|4321\n",
            argv[0]
        );
        return 64;
    }
    selected_order = atoi(argv[3]);

    status = db_create(&source, NULL, 0);
    if (status != 0) {
        fprintf(stderr, "source db_create: %s\n", db_strerror(status));
        goto cleanup;
    }
    status = source->open(
        source,
        NULL,
        argv[1],
        NULL,
        DB_UNKNOWN,
        DB_RDONLY,
        0
    );
    if (status != 0) {
        fprintf(stderr, "source open: %s\n", db_strerror(status));
        goto cleanup;
    }

    status = db_create(&destination, NULL, 0);
    if (status != 0) {
        fprintf(stderr, "destination db_create: %s\n", db_strerror(status));
        goto cleanup;
    }
    status = destination->set_lorder(destination, selected_order);
    if (status != 0) {
        fprintf(stderr, "destination set_lorder: %s\n", db_strerror(status));
        goto cleanup;
    }
    status = destination->open(
        destination,
        NULL,
        argv[2],
        NULL,
        DB_HASH,
        DB_CREATE | DB_TRUNCATE,
        0600
    );
    if (status != 0) {
        fprintf(stderr, "destination open: %s\n", db_strerror(status));
        goto cleanup;
    }

    status = source->cursor(source, NULL, &cursor, 0);
    if (status != 0) {
        fprintf(stderr, "source cursor: %s\n", db_strerror(status));
        goto cleanup;
    }
    memset(&key, 0, sizeof(key));
    memset(&value, 0, sizeof(value));
    while ((status = cursor->get(cursor, &key, &value, DB_NEXT)) == 0) {
        status = destination->put(
            destination,
            NULL,
            &key,
            &value,
            DB_NOOVERWRITE
        );
        if (status != 0) {
            fprintf(stderr, "destination put: %s\n", db_strerror(status));
            goto cleanup;
        }
        record_count++;
        memset(&key, 0, sizeof(key));
        memset(&value, 0, sizeof(value));
    }
    if (status != DB_NOTFOUND) {
        fprintf(stderr, "source cursor get: %s\n", db_strerror(status));
        goto cleanup;
    }
    result = 0;

cleanup:
    if (cursor != NULL) {
        status = cursor->close(cursor);
        if (status != 0) {
            fprintf(stderr, "cursor close: %s\n", db_strerror(status));
            result = 1;
        }
    }
    if (destination != NULL) {
        status = destination->close(destination, 0);
        if (status != 0) {
            fprintf(stderr, "destination close: %s\n", db_strerror(status));
            result = 1;
        }
    }
    if (source != NULL) {
        status = source->close(source, 0);
        if (status != 0) {
            fprintf(stderr, "source close: %s\n", db_strerror(status));
            result = 1;
        }
    }

    if (result == 0) {
        printf(
            "HDB07C repacked records: %lu; byte order: %d\n",
            record_count,
            selected_order
        );
    }
    return result;
}
