<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SalesDetails.aspx.cs" Inherits="TalTun.WCF.WebClient.SalesDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CPHMain" runat="server">
    <asp:Repeater ID="RepInfoOne" runat="server">
        <ItemTemplate>
            <div class="prodWraper">
                 <div class="prodImg">
                    <img src="<%# Eval("ProdImg")%>" />
                </div>
                <br />
              <div class="prodDetail"> 
                <%# Eval("ProdName")%> 
               </div>
                <br />
                <div class="prodPrice">
                 INR <%# Eval("ProdPrice")%> 
            </div>
                <div class="prodPrice">
                 Qty <%# Eval("Qnt")%> 
            </div>
         </ItemTemplate>
    </asp:Repeater>
</asp:Content>
