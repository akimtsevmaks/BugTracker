BEGIN;

ALTER ROLE bugtracker_api IN DATABASE bugtracker_dev
    SET search_path TO bugtracker;

ALTER ROLE bugtracker_migrator IN DATABASE bugtracker_dev
    SET search_path TO bugtracker;

COMMIT;