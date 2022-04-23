<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Predictor.aspx.cs" Inherits="PredictorWeb.Predictor" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Prediction</title>
    <link href="https://kendo.cdn.telerik.com/2022.1.301/styles/kendo.common.min.css" rel="stylesheet" />
    <link href="https://kendo.cdn.telerik.com/2022.1.301/styles/kendo.default.min.css" rel="stylesheet" />
    <script src="https://kendo.cdn.telerik.com/2022.1.301/js/jquery.min.js"></script>
    <script src="https://kendo.cdn.telerik.com/2022.1.301/js/kendo.all.min.js"></script>
    <style>
        #ListViewBox
        {
            width:1650px;
            height:1200px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div style="margin-left: auto; margin-right: auto; text-align: center;">
         <asp:Label ID="Label2" runat="server" Text="Blood Culture Predictor" Font-Bold="True" Font-Size="25"></asp:Label><br />
        </div>
        <div>
            <asp:FileUpload ID="FileUpload1" runat="server" Width="50" />
            <asp:Button ID="PredictButton" runat="server" Text="Predict" OnClick="Button1_Click" />
            <asp:DropDownList ID="DropDownList1" runat="server" style="margin-bottom: 0px" AutoPostBack="True" OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" Width="150px">
            </asp:DropDownList>
            <asp:Label ID="Label1" runat="server" Text="請選檔案"></asp:Label>
            <%--<asp:Image ID="Image1" runat="server"/>--%>
            <br />
            <br />
            <div id="ListViewBox">
                <asp:ListView ID="ListView1" runat="server" >
                    <ItemTemplate>
                        <asp:Image ID="Image" runat="server" ImageUrl='<%# Container.DataItem %>' />
                    </ItemTemplate>
                </asp:ListView>
            </div>
        </div>
    </form>
    <script type="text/javascript" language="javascript">
        $("#PredictButton").kendoButton({});
        $("#FileUpload1").kendoUpload({"multiple": false}).data("kendoUpload");
        $("#DropDownList1").kendoDropDownList({});
    </script>
</body>
</html>
