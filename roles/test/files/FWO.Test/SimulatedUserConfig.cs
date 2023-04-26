﻿using FWO.Config.Api;

namespace FWO.Test
{
    internal class SimulatedUserConfig : UserConfig
    {
        public Dictionary<string, string> DummyTranslate = new Dictionary<string, string>()
        {
            {"Rules","Rules Report"},
            {"ResolvedRules","Rules Report (resolved)"},
            {"ResolvedRulesTech","Rules Report (technical)"},
            {"ResolvedChanges","Changes Report (resolved)"},
            {"ResolvedChangesTech","Changes Report (technical)"},
            {"date_of_config","Time of configuration"},
            {"generated_on","Generated on"},
            {"negated","not"},
            {"users","Users"},
            {"rule_added","Rule added"},
            {"rule_deleted","Rule deleted"},
            {"rule_modified","Rule modified"},
            {"deleted","deleted"},
            {"added","added"},
            {"change_time","Change Time"},
            {"change_type","Change Type"},
            {"number","No."},
            {"name","Name"},
            {"source_zone","Source Zone"},
            {"source","Source"},
            {"destination_zone","Destination Zone"},
            {"destination","Destination"},
            {"services","Services"},
            {"action","Action"},
            {"track","Track"},
            {"enabled","Enabled"},
            {"uid","Uid"},
            {"comment","Comment"},
            {"type","Type"},
            {"ip_address","IP Address"},
            {"members","Members"},
            {"network_objects","Network Objects"},
            {"network_services","Network Services"},
            {"protocol","Protocol"},
            {"port","Port"}
        };

        public override string GetText(string key)
        {
            return DummyTranslate[key];
        }
    }
}
