<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GEChalkboard.aspx.cs" Inherits="PeriGen.Patterns.GE.Web.GEChalkboard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="expires" content="-1" />
	<meta http-equiv="pragma" content="no-cache" />
	<meta http-equiv="cache-control" content="no-cache" />
	<meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
	<title>PeriGen PeriCALM® Patterns™ (GE) Chalkboard</title>
	<style type="text/css">
table
{
	font-family: "Lucida Sans Unicode", "Lucida Grande", Sans-Serif;
	font-size: 12px;
	text-align: center;
	border-collapse: collapse;
}
table th
{
	text-align: left;
	font-size: 13px;
	font-weight: normal;
	padding: 8px;
	background: #b9c9fe;
	border-top: 4px solid #aabcfe;
	border-bottom: 1px solid #fff;
	border-left: 1px solid #fff;
	color: #039;
}
table td
{
	text-align : left;
	padding: 8px;
	border-bottom: 1px solid #fff;
	border-left: 1px solid #fff;
	color: #669;
	border-top: 1px solid transparent;
}
	</style>
	
</head>
<body>
	<form id="form1" runat="server">
	<asp:PlaceHolder ID="PlaceHolderChart" runat="server" />
	</form>
</body>
</html>
