ALTER TABLE request.reqelement ALTER COLUMN original_nat_id TYPE bigint;
ALTER TABLE request.reqelement ADD COLUMN IF NOT EXISTS device_id int;
ALTER TABLE request.reqelement ADD COLUMN IF NOT EXISTS rule_uid varchar;
ALTER TABLE request.reqelement DROP CONSTRAINT IF EXISTS request_reqelement_device_foreign_key;
ALTER TABLE request.reqelement ADD CONSTRAINT request_reqelement_device_foreign_key FOREIGN KEY (device_id) REFERENCES device(dev_id) ON UPDATE RESTRICT ON DELETE CASCADE;

ALTER TABLE request.implelement ALTER COLUMN original_nat_id TYPE bigint;
ALTER TABLE request.implelement ADD COLUMN IF NOT EXISTS rule_uid varchar;

ALTER TYPE rule_field_enum ADD VALUE IF NOT EXISTS 'rule';

insert into config (config_key, config_value, config_user) VALUES ('recAutocreateDeleteTicket', 'False', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleTicketTitle', 'Ticket Title', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleTicketReason', 'Ticket Reason', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleReqTaskTitle', 'Task Title', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleReqTaskReason', 'Task Reason', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleTicketPriority', '3', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recDeleteRuleInitState', '0', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recCheckEmailSubject', 'Upcoming rule recertifications', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recCheckEmailUpcomingText', 'The following rules are upcoming to be recertified:', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recCheckEmailOverdueText', 'The following rules are overdue to be recertified:', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recCheckActive', 'False', 0) ON CONFLICT DO NOTHING;

ALTER TABLE owner ADD COLUMN IF NOT EXISTS last_recert_check Timestamp;
ALTER TABLE owner ADD COLUMN IF NOT EXISTS recert_check_params Varchar;

drop index if exists only_one_future_recert_per_owner_per_rule;
create unique index if not exists only_one_future_recert_per_owner_per_rule on recertification(owner_id,rule_metadata_id,recert_date) 
    where recert_date IS NULL;

ALTER TABLE owner_network DROP CONSTRAINT IF EXISTS owner_network_ip_unique;
ALTER TABLE owner_network ADD CONSTRAINT owner_network_ip_unique UNIQUE (owner_id, ip);

ALTER TABLE owner DROP COLUMN IF EXISTS next_recert_date;

Create index IF NOT EXISTS idx_object04 on object (obj_ip);
Create index IF NOT EXISTS idx_rule04 on rule (action_id);

-- replacing view by materialized view
CREATE OR REPLACE FUNCTION purge_view_rule_with_owner () RETURNS VOID AS $$
DECLARE
    r_temp_record RECORD;
BEGIN
    select INTO r_temp_record schemaname, viewname from pg_catalog.pg_views
    where schemaname NOT IN ('pg_catalog', 'information_schema') and viewname='view_rule_with_owner'
    order by schemaname, viewname;
    IF FOUND THEN
        DROP VIEW IF EXISTS view_rule_with_owner CASCADE;
    END IF;
    DROP MATERIALIZED VIEW IF EXISTS view_rule_with_owner CASCADE;
    RETURN;
END;
$$ LANGUAGE plpgsql;

SELECT * FROM purge_view_rule_with_owner ();
DROP FUNCTION purge_view_rule_with_owner();

-- LargeOwnerChange: uncomment to disable triggers (e.g. for large installations without recert needs)
-- ALTER TABLE owner DISABLE TRIGGER owner_change;
-- ALTER TABLE owner_network DISABLE TRIGGER owner_network_change;

CREATE MATERIALIZED VIEW view_rule_with_owner AS
	SELECT DISTINCT r.rule_num_numeric, r.track_id, r.action_id, r.rule_from_zone, r.rule_to_zone, r.dev_id, r.mgm_id, r.rule_uid, uno.rule_id, uno.owner_id, uno.owner_name, uno.rule_last_certified, uno.rule_last_certifier, 
	rule_action, rule_name, rule_comment, rule_track, rule_src_neg, rule_dst_neg, rule_svc_neg,
	rule_head_text, rule_disabled, access_rule, xlate_rule, nat_rule,
	string_agg(DISTINCT match_in || ':' || matching_ip::VARCHAR, '; ' order by match_in || ':' || matching_ip::VARCHAR desc) as matches,
	recert_interval
	FROM ( SELECT DISTINCT * FROM v_rule_with_src_owner UNION SELECT DISTINCT * FROM v_rule_with_dst_owner ) AS uno
	LEFT JOIN rule AS r USING (rule_id)
	GROUP BY rule_id, owner_id, owner_name, rule_last_certified, rule_last_certifier, r.rule_from_zone, r.rule_to_zone,  recert_interval,
		r.dev_id, r.mgm_id, r.rule_uid, rule_num_numeric, track_id, action_id, 	rule_action, rule_name, rule_comment, rule_track, rule_src_neg, rule_dst_neg, rule_svc_neg,
		rule_head_text, rule_disabled, access_rule, xlate_rule, nat_rule;

------------
-- add new super owner

DELETE FROM owner WHERE name='defaultOwner_demo';
UPDATE owner SET is_default=false WHERE id>0;   -- idempotence
INSERT INTO owner (id, name, dn, group_dn, is_default, recert_interval, app_id_external) 
VALUES    (0, 'super-owner', 'uid=admin,ou=tenant0,ou=operator,ou=user,dc=fworch,dc=internal', 'group-dn-for-super-owner', true, 365, 'NONE')
ON CONFLICT DO NOTHING; 

-------------------------
-- add recert refresh trigger

create or replace function refresh_view_rule_with_owner()
returns trigger language plpgsql
as $$
begin
    refresh materialized view view_rule_with_owner;
    return null;
end $$;

drop trigger IF exists refresh_view_rule_with_owner_delete_trigger ON recertification CASCADE;

create trigger refresh_view_rule_with_owner_delete_trigger
after delete on recertification for each statement 
execute procedure refresh_view_rule_with_owner();

ALTER TABLE owner DROP CONSTRAINT IF EXISTS owner_name_key;
ALTER TABLE owner ADD CONSTRAINT owner_name_key UNIQUE (name);
ALTER TABLE owner DROP CONSTRAINT IF EXISTS owner_app_id_external_key;
ALTER TABLE owner ADD CONSTRAINT owner_app_id_external_key UNIQUE (app_id_external);
ALTER TABLE owner ALTER COLUMN app_id_external DROP NOT NULL;
insert into config (config_key, config_value, config_user) VALUES ('recRefreshStartup', 'False', 0) ON CONFLICT DO NOTHING;
insert into config (config_key, config_value, config_user) VALUES ('recRefreshDaily', 'False', 0) ON CONFLICT DO NOTHING;
