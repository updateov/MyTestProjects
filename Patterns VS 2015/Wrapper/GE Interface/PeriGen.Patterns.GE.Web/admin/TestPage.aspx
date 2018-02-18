<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPage.aspx.cs" Inherits="PeriGen.Patterns.GE.Web.TestGEService" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>PeriGen PeriCALM® Patterns™ Test Page</title>
	<style type="text/css">
		.style1
		{
			height: 30px;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server">
	<label runat="server" style="font-size: x-large; font-weight: bold; font-variant: normal;
		text-transform: capitalize; color: #000080">
		PeriGen PeriCALM® Patterns™ GE Test Page<br />
		&nbsp;</label><table style="width: 960px;">
			<tr>
				<td>
					Server URL
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtServerURL" Width="500px">http://localhost/PatternsDataFeed/</asp:TextBox>
					<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtServerURL"
						Display="None" ErrorMessage="Server URL cannot be empty"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Patient ID
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtPatientID" Width="500px"></asp:TextBox>
					<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtPatientID"
						Display="None" ErrorMessage="Patient ID cannot be empty"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td class="style1">
					User ID
				</td>
				<td class="style1">
					<asp:TextBox runat="server" ID="txtUserID" Width="500px">PeriGen</asp:TextBox>
					<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="txtUserID"
						Display="None" ErrorMessage="User ID cannot be empty"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					User Name
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtUserName" Width="500px">PeriGen Support</asp:TextBox>
					<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="txtUserName"
						Display="None" ErrorMessage="User Name cannot be empty"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Refresh (sec)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtRefresh" Width="125px">4</asp:TextBox>
					<asp:RangeValidator ID="refreshValidator" runat="server" ControlToValidate="txtRefresh"
						Display="None" ErrorMessage="Refresh must be a numeric value (from 1 to 60)"
						MaximumValue="60" MinimumValue="1" Type="Integer"></asp:RangeValidator>
					&nbsp;
					<asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="txtRefresh"
						ErrorMessage="Refresh cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Permissions
				</td>
				<td>
					<asp:DropDownList ID="cmbPermissions" runat="server" Width="125px">
						<asp:ListItem Value="readonly" Selected="True"></asp:ListItem>
						<asp:ListItem Value="fullaccess"></asp:ListItem>
					</asp:DropDownList>
				</td>
			</tr>
			<tr>
				<td>
					Window duration (min)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtWinDuration" Width="125px">10</asp:TextBox>
					<asp:RangeValidator ID="RangeValidator1" runat="server" ControlToValidate="txtWinDuration"
						ErrorMessage="Window duration must be a numeric value (from 1 to 60)" MaximumValue="60"
						MinimumValue="1" Type="Integer" Display="None"></asp:RangeValidator>
					&nbsp;
					<asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="txtWinDuration"
						ErrorMessage="Window duration cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Graph max (count)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtGraphMax" Width="125px">10</asp:TextBox>
					<asp:RangeValidator ID="RangeValidator2" runat="server" ControlToValidate="txtGraphMax"
						ErrorMessage="Graph max must be a numeric value (from 1 to 60)" MaximumValue="60"
						MinimumValue="1" Type="Integer" Display="None"></asp:RangeValidator>
					&nbsp;
					<asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="txtGraphMax"
						ErrorMessage="Graph max cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Stage 1 (count)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtStage1" Width="125px">0</asp:TextBox>
					<asp:RangeValidator ID="RangeValidator3" runat="server" ControlToValidate="txtStage1"
						ErrorMessage="Stage 1 must be a numeric value (from 0 to 60, 0 to disable)" MaximumValue="60"
						MinimumValue="0" Type="Integer" Display="None"></asp:RangeValidator>
					&nbsp;
					<asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="txtStage1"
						ErrorMessage="Stage 1 cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Stage 2 (min)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtStage2" Width="125px">0</asp:TextBox>
					<asp:RangeValidator ID="RangeValidator4" runat="server" ControlToValidate="txtStage2"
						ErrorMessage="Stage 2 must be a numeric value (from 0 to 60, 0 to disable)" MaximumValue="60"
						MinimumValue="0" Type="Integer" Display="None"></asp:RangeValidator>
					&nbsp;
					<asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="txtStage2"
						ErrorMessage="Stage 2 cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					Timeout dialogs (min)
				</td>
				<td>
					<asp:TextBox runat="server" ID="txtTimeoutDlg" Width="125px">0</asp:TextBox>
					<asp:RangeValidator ID="RangeValidator5" runat="server" ControlToValidate="txtStage2"
						ErrorMessage="Timeout must be a numeric value (from 0 to 60, 0 to disable)" MaximumValue="60"
						MinimumValue="0" Type="Integer" Display="None"></asp:RangeValidator>
					<asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="txtStage2"
						ErrorMessage="Tiemout cannot be empty" Display="None"></asp:RequiredFieldValidator>
				</td>
			</tr>
			<tr>
				<td>
					&nbsp;&nbsp;
				</td>
				<td>
					&nbsp;&nbsp;
				</td>
			</tr>
			<tr>
				<td>
					<asp:Button ID="btnOpenPage" runat="server" Text="GO!" Width="100px" OnClick="btnOpenPage_Click"
						Style="height: 26px" />
				</td>
				<td>
					<asp:Label ID="lblResult" runat="server"></asp:Label>
				</td>
			</tr>
			<tr>
				<td colspan="2">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" />
				</td>
			</tr>
		</table>
	</form>
</body>
</html>
