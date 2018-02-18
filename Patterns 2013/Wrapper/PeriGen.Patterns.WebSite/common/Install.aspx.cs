using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Web.UI.HtmlControls;

namespace PeriGen.Patterns.WebSite.common
{
    public partial class Install : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) 
            {
                //App
                var app = Request.QueryString["app"];
                if (string.IsNullOrEmpty(app) || string.IsNullOrWhiteSpace(app)) 
                {
                    Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>PeriCALM® Patterns™</h1><br/><h2><span style='color:red'>Invalid parameters.</span></h2></div>");
                    Response.End();
                    return;                    
                }

                app=app.Trim().ToUpper();

                if (app == "PATTERNS") 
                {
                    HtmlGenericControl objectTag = new HtmlGenericControl("object");
                    objectTag.Attributes["name"] = "PatChart";
                    objectTag.Attributes["classid"] = PatternsWebSiteSettings.Instance.EnableCheckList ? 
                                                        "CLSID:822D5CDC-3062-4C71-8FA5-7156E38B2F1C" : 
                                                        "CLSID:00172661-82A6-4C68-8585-D54E46A07CCF";
                    objectTag.Attributes["height"] = "100%";
                    //objectTag.Attributes["codebase"] = "./PeriCALMPatternsOEMChart.cab#version=" + SettingsManager.GetValue("VersionPatterns").Replace('.', ',');
                    objectTag.Attributes["codebase"] = PatternsWebSiteSettings.Instance.ActiveXClient + "#version=" + PatternsWebSiteSettings.Instance.VersionPatterns.Replace('.', ',');
                    objectTag.Attributes["width"] = "100%";
                    objectTag.Attributes["border"] = "0";

                    this.PlaceHolderChart.Controls.Add(objectTag);
                }
                else if (app == "CURVE")
                {
                    //Add to the response stream
                    Response.Cookies.Add(new HttpCookie("PeriGenCurveDataFlag", "1") { Expires = DateTime.UtcNow.AddSeconds(5) });

                    Response.Redirect("PeriCALM.Patterns.Curve.UI.Chart.xbap");
                }
                else 
                {
                    Response.Write("<div style=\"border: 2px solid #000000; padding: 2px 10px 10px 10px; margin: 10px; background: #FFFFFF;\"><h1>PeriCALM® Patterns™</h1><br/><h2><span style='color:red'>Invalid parameters.</span></h2></div>");
                    Response.End();
                    return;                                    
                }

            }
        }
    }
}