
ALTER TABLE "rule_to" DROP CONSTRAINT if exists rule_to_pkey;
ALTER table "rule_to" ADD column IF NOT EXISTS user_id BIGINT;
ALTER table "rule_to" ADD column IF NOT EXISTS rule_to_id BIGSERIAL primary key;
Alter table "rule_to" drop constraint if exists rule_to_user_id_fkey;
Alter table "rule_to" drop constraint if exists rule_to_user_id_usr_user_id;
Alter table "rule_to" add constraint rule_to_user_id_usr_user_id FOREIGN KEY 
    ("user_id") references "usr" ("user_id") on update restrict on delete cascade;

ALTER TABLE "log_data_issue" ADD COLUMN IF NOT EXISTS "user_id" INTEGER DEFAULT 0;

ALTER TABLE "log_data_issue" DROP CONSTRAINT IF EXISTS log_data_issue_uiuser_uiuser_id_fkey CASCADE;
Alter table "log_data_issue" add CONSTRAINT log_data_issue_uiuser_uiuser_id_fkey foreign key ("user_id") references "uiuser" ("uiuser_id") on update restrict on delete cascade;

ALTER TABLE "log_data_issue" DROP CONSTRAINT IF EXISTS log_data_issue_import_control_control_id_fkey;
ALTER TABLE "log_data_issue" ADD CONSTRAINT log_data_issue_import_control_control_id_fkey FOREIGN KEY 
	("import_id") REFERENCES "import_control" ("control_id") ON UPDATE RESTRICT ON DELETE CASCADE;

Create table IF NOT EXISTS "alert"
(
	"alert_id" BIGSERIAL,
	"ref_log_id" BIGINT,
	"ref_alert_id" BIGINT,
	"source" VARCHAR NOT NULL,
	"title" VARCHAR,
	"description" VARCHAR,
	"alert_mgm_id" INTEGER,
	"alert_dev_id" INTEGER,
	"alert_timestamp" TIMESTAMP DEFAULT NOW(),
	"user_id" INTEGER DEFAULT 0,
	"ack_by" INTEGER,
	"ack_timestamp" TIMESTAMP,
	"json_data" json,
 primary key ("alert_id")
);

ALTER TABLE "alert" DROP CONSTRAINT IF EXISTS "alert_ref_log_id_log_data_issue_data_issue_id_fkey" CASCADE;
Alter table "alert" add CONSTRAINT alert_ref_log_id_log_data_issue_data_issue_id_fkey foreign key ("ref_log_id") references "log_data_issue" ("data_issue_id") on update restrict on delete cascade;
ALTER TABLE "alert" DROP CONSTRAINT IF EXISTS "alert_user_id_uiuser_uiuser_id_fkey" CASCADE;
Alter table "alert" add CONSTRAINT alert_user_id_uiuser_uiuser_id_fkey foreign key ("user_id") references "uiuser" ("uiuser_id") on update restrict on delete cascade;
ALTER TABLE "alert" DROP CONSTRAINT IF EXISTS "alert_ack_by_uiuser_uiuser_id_fkey" CASCADE;
Alter table "alert" add CONSTRAINT alert_ack_by_uiuser_uiuser_id_fkey foreign key ("ack_by") references "uiuser" ("uiuser_id") on update restrict on delete cascade;
ALTER TABLE "alert" DROP CONSTRAINT IF EXISTS "alert_alert_mgm_id_management_mgm_id_fkey" CASCADE;
Alter table "alert" add CONSTRAINT alert_alert_mgm_id_management_mgm_id_fkey foreign key ("alert_mgm_id") references "management" ("mgm_id") on update restrict on delete cascade;
ALTER TABLE "alert" DROP CONSTRAINT IF EXISTS "alert_alert_dev_id_device_dev_id_fkey" CASCADE;
Alter table "alert" add CONSTRAINT alert_alert_dev_id_device_dev_id_fkey foreign key ("alert_dev_id") references "device" ("dev_id") on update restrict on delete cascade;

ALTER TABLE "alert" ADD COLUMN IF NOT EXISTS "alert_code" INTEGER;

insert into config (config_key, config_value, config_user) VALUES ('messageViewTime', '7', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('dailyCheckStartAt', '00:00:00', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('autoDiscoverStartAt', '00:00:00', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('autoDiscoverSleepTime', '24', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('minCollapseAllDevices', '15', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('pwMinLength', '10', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('pwUpperCaseRequired', 'False', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('pwLowerCaseRequired', 'False', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('pwNumberRequired', 'False', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('pwSpecialCharactersRequired', 'False', 0) ON CONFLICT DO NOTHING;
