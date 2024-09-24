-- path analysis config setting
insert into config (config_key, config_value, config_user) VALUES ('reqActivatePathAnalysis', 'True', 0) ON CONFLICT DO NOTHING;

-- adding azure devices
insert into stm_dev_typ (dev_typ_id,dev_typ_name,dev_typ_version,dev_typ_manufacturer,dev_typ_predef_svc,dev_typ_is_multi_mgmt,dev_typ_is_mgmt,is_pure_routing_device)
     VALUES (19,'Azure','2022ff','Microsoft','',false,true,false) ON CONFLICT DO NOTHING;
insert into stm_dev_typ (dev_typ_id,dev_typ_name,dev_typ_version,dev_typ_manufacturer,dev_typ_predef_svc,dev_typ_is_multi_mgmt,dev_typ_is_mgmt,is_pure_routing_device)
    VALUES (20,'Azure Firewall','2022ff','Microsoft','',false,false,false) ON CONFLICT DO NOTHING;

ALTER TABLE management ADD COLUMN IF NOT EXISTS cloud_tenant_id VARCHAR;
ALTER TABLE management ADD COLUMN IF NOT EXISTS cloud_subscription_id VARCHAR;

ALTER TABLE import_credential ADD COLUMN IF NOT EXISTS cloud_client_id VARCHAR;
ALTER TABLE import_credential ADD COLUMN IF NOT EXISTS cloud_client_secret VARCHAR;

--------------------------------------------------------------
-- owner demo data 
--------------------------------------------------------------

-- adding unique constraint for owner.name
ALTER TABLE owner DROP CONSTRAINT IF EXISTS owner_name_unique_in_tenant;
ALTER TABLE owner ADD CONSTRAINT owner_name_unique_in_tenant UNIQUE ("name","tenant_id");

-- CREATE OR REPLACE VIEW v_active_access_rules AS 
-- 	SELECT * FROM rule r
-- 	WHERE r.active AND r.access_rule AND NOT r.rule_disabled AND r.rule_head_text IS NULL;

DROP VIEW IF EXISTS v_active_access_allow_rules CASCADE; 
CREATE OR REPLACE VIEW v_active_access_allow_rules AS 
	SELECT * FROM rule r
	WHERE r.active AND 					-- only show current (not historical) rules 
		r.access_rule AND 				-- only show access rules (no NAT)
		r.rule_head_text IS NULL AND 	-- do not show header rules
		NOT r.rule_disabled AND 		-- do not show disabled rules
		NOT r.action_id IN (2,3,7);		-- do not deal with deny rules

DROP VIEW IF EXISTS v_rule_with_src_owner CASCADE; 
CREATE OR REPLACE VIEW v_rule_with_src_owner AS 
	SELECT r.rule_id, owner.id as owner_id, owner_network.ip as matching_ip, 'source' AS match_in, owner.name as owner_name, 
		rule_metadata.rule_last_certified, rule_last_certifier
	FROM v_active_access_allow_rules r
	LEFT JOIN rule_from ON (r.rule_id=rule_from.rule_id)
	LEFT JOIN objgrp_flat of ON (rule_from.obj_id=of.objgrp_flat_id)
	LEFT JOIN object o ON (o.obj_typ_id<>2 AND of.objgrp_flat_member_id=o.obj_id)
	LEFT JOIN owner_network ON (o.obj_ip>>=owner_network.ip OR o.obj_ip<<=owner_network.ip)
	LEFT JOIN owner ON (owner_network.owner_id=owner.id)
	LEFT JOIN rule_metadata ON (r.rule_uid=rule_metadata.rule_uid AND r.dev_id=rule_metadata.dev_id)
	GROUP BY r.rule_id, matching_ip, owner.id, owner.name, rule_metadata.rule_last_certified, rule_last_certifier;
	
DROP VIEW IF EXISTS v_rule_with_dst_owner CASCADE; 
CREATE OR REPLACE VIEW v_rule_with_dst_owner AS 
	SELECT r.rule_id, owner.id as owner_id, owner_network.ip as matching_ip, 'destination' AS match_in, owner.name as owner_name, 
		rule_metadata.rule_last_certified, rule_last_certifier
	FROM v_active_access_allow_rules r
	LEFT JOIN rule_to ON (r.rule_id=rule_to.rule_id)
	LEFT JOIN objgrp_flat of ON (rule_to.obj_id=of.objgrp_flat_id)
	LEFT JOIN object o ON (o.obj_typ_id<>2 AND of.objgrp_flat_member_id=o.obj_id)
	LEFT JOIN owner_network ON (o.obj_ip>>=owner_network.ip OR o.obj_ip<<=owner_network.ip)
	LEFT JOIN owner ON (owner_network.owner_id=owner.id)
	LEFT JOIN rule_metadata ON (r.rule_uid=rule_metadata.rule_uid AND r.dev_id=rule_metadata.dev_id)
	GROUP BY r.rule_id, matching_ip, owner.id, owner.name, rule_metadata.rule_last_certified, rule_last_certifier;

--necessary when changing materialized/non-mat. view
/* CREATE OR REPLACE FUNCTION purge_view_rule_with_owner () RETURNS VOID AS $$
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
*/

CREATE OR REPLACE VIEW view_rule_with_owner AS 
	SELECT DISTINCT r.rule_num_numeric, r.track_id, r.action_id, r.rule_from_zone, r.rule_to_zone, r.dev_id, r.mgm_id, r.rule_uid, uno.rule_id, uno.owner_id, uno.owner_name, uno.rule_last_certified, uno.rule_last_certifier, 
	rule_action, rule_name, rule_comment, rule_track, rule_src_neg, rule_dst_neg, rule_svc_neg,
	rule_head_text, rule_disabled, access_rule, xlate_rule, nat_rule,
	string_agg(DISTINCT match_in || ':' || matching_ip::VARCHAR, '; ' order by match_in || ':' || matching_ip::VARCHAR desc) as matches
	FROM ( SELECT DISTINCT * FROM v_rule_with_src_owner UNION SELECT DISTINCT * FROM v_rule_with_dst_owner ) AS uno
	LEFT JOIN rule AS r USING (rule_id)
	GROUP BY rule_id, owner_id, owner_name, rule_last_certified, rule_last_certifier, r.rule_from_zone, r.rule_to_zone, 
		r.dev_id, r.mgm_id, r.rule_uid, rule_num_numeric, track_id, action_id, 	rule_action, rule_name, rule_comment, rule_track, rule_src_neg, rule_dst_neg, rule_svc_neg,
		rule_head_text, rule_disabled, access_rule, xlate_rule, nat_rule;

ALTER TABLE owner_network drop constraint IF EXISTS owner_network_unique_in_tenant;
