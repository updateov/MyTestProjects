<%@ Page Language="C#" AutoEventWireup="true" ValidateRequest="false" CodeBehind="Help.aspx.cs"
	Inherits="PeriGen.Patterns.WebSite.Help" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" style="margin: 0; padding: 0; height: 100%;
overflow: auto;">
<head runat="server">
	<title>PeriCALM® Decision Support HELP</title>
	<link rel="shortcut icon" href="..\app.ico" />
</head>
<body style="margin: 0; padding: 0; height: 100%; width: 100%; overflow: hidden;">
	<table border="0" width="100%" cellspacing="0" cellpadding="5">
		<tr>
			<td>
				<table bgcolor="Grey" border="0" width="100%" cellspacing="0" cellpadding="5">
					<tr>
						<td width="50%" valign="top" align="left">
							<img border="0" src="img/Logo_Perigen.png" alt="PERIGEN" />
						</td>
						<td width="50%" valign="bottom" align="right">
							<img border="0" src="img/Logo_EmpoweringClinicians.png" alt="Empowering clinicians" />
						</td>
					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<td>
				<div runat="server" id="PatternsDiv">
					<table width="100%">
						<tr>
							<td width="5%">
							</td>
							<td width="95%" valign="top" align="left">
								<br />
								<br />
								<p style="text-align: left; font-family: Arial,Helvetica,Sans-Serif; font-size: 24px;
									font-weight: 800; color: #74C8E2;">
									PeriCALM<sup>®</sup> PATTERNS<sup>™</sup></p>
							</td>
						</tr>
						<tr>
							<td width="5%">
							</td>
							<td width="900" valign="top" align="left">
								<p style="text-align: left; font-family: Arial,Helvetica,Sans-Serif; font-size: 12px;
									color: #000000;">
									From data collected through a fetal monitor, the <b>PeriCALM Patterns</b> detects
									and analyzes fetal heart rate accelerations, late, early, variable and prolonged
									decelerations, as well as uterine contractions, and displays these as colored markings.
									Furthermore, <b>PeriCALM Patterns</b> can calculate baseline, baseline variability,
									Montevideo units and contraction interval averages in a sliding window, as well
									as warn against persistence in uterine tachysystole.
									<br />
									See the
									<img src="img/pdficon_small.png" alt="PDF" />
									<b><a href="PeriCALM PATTERNS User Guide.pdf" target="_blank">PeriCALM Patterns User
										Guide</a></b> for further details.
									<br />
									<br />
									<br />
									<br />
								</p>
							</td>
						</tr>
					</table>
				</div>
				<div runat="server" id="CurveDiv">
					<table width="100%">
						<tr>
							<td width="5%">
							</td>
							<td width="95%" valign="top" align="left">
								<p style="text-align: left; font-family: Arial,Helvetica,Sans-Serif; font-size: 24px;
									font-weight: 800; color: #74C8E2;">
									PeriCALM<sup>®</sup> CURVE<sup>™</sup></p>
							</td>
						</tr>
						<tr>
							<td width="5%">
							</td>
							<td width="900" valign="top" align="left">
								<p style="text-align: left; font-family: Arial,Helvetica,Sans-Serif; font-size: 12px;
									color: #000000;">
									The <b>PeriCALM Curve</b> uses statistical formulas to show how cervical dilation
									generally changes during labor, based on data from a reference group of mothers
									who delivered vaginally. <b>PeriCALM Curve</b> calculations produce a graph which
									shows the average expected cervical dilation, thus clinicians can compare the observed
									dilatation of a specific mother under their care to the dilation pattern from women
									with similar labor related conditions from the reference group.
									<br />
									See the
									<img src="img/pdficon_small.png" alt="PDF" />
									<b><a href="PeriCALM CURVE User Guide.pdf" target="_blank">PeriCALM Curve User Guide</a></b>
									for further details.
									<br />
									<br />
									<br />
									<br />
								</p>
							</td>
						</tr>
					</table>
				</div>
				<div runat="server" id="AdobeDiv">
					<table width="100%">
						<tr>
							<td width="5%">
							</td>
							<td align="left">
								<a href="http://www.adobe.com/go/getreader" target="_blank">
									<img src="img/get_adobe_reader.png" alt="Get Adobe Reader" />
								</a>
							</td>
						</tr>
					</table>
				</div>
			</td>
		</tr>
	</table>
</body>
</html>
