<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="html.aspx.cs" Inherits="WeatherApp.html" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css" />
    <link rel='StyleSheet' type='text/css' href='StyleSheet.css' />
    <title></title>
    <script type="text/javascript">
        function searchLocation(location) {
            document.getElementById('<%= LocationInput.ClientID %>').value = location;
            document.getElementById('<%= SearchButton.ClientID %>').click();
           
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main" id="mainDiv" runat="server">
            <asp:Label ID="LogLabel" runat="server" Text=""></asp:Label>
            <div class="flex">
                <div class="left">
                    <div class="title">
                        <h1 style="display: flex;">
                            <asp:Label ID="Temperature" runat="server"></asp:Label>
                            <span>&deg;</span>
                        </h1>
                    </div>
                    <div class="mainLocation">
                        <asp:Label ID="LocationLabel" runat="server" CssClass="location" Text=""></asp:Label>
                        <h2>
                            <asp:Label ID="Location" runat="server" CssClass="location" Text=""></asp:Label>
                        </h2>
                        <p>
                            <asp:Label ID="Date" runat="server" CssClass="time" Text=""></asp:Label>
                        </p>
                    </div>
                    <div class="weatherIcon">
                        <i class="fa fa-cloud"></i>
                        <p>Cloudy</p>
                    </div>
                </div>
                <div class="right">
                    <div class="containerDetailWeather">
                        <div class="search">
                            <input class="input" type="text" placeholder="Another location" runat="server" id="LocationInput" />
                            <div class="searchIcon">
                                <asp:LinkButton Style="text-decoration: none;" ID="SearchButton" runat="server" CssClass="searchButton" OnClick="SearchIcon_Click">
                                <i class="fa fa-search"></i>
                                </asp:LinkButton>
                            </div>
                        </div>
                        
                        <!-- showing the histories -->
                        <ul style="height: 22vh;">
                            <asp:Repeater ID="LocationRepeater" runat="server">
                                <ItemTemplate>
                                    <li>
                                        <a href="javascript:void(0);" onclick="searchLocation('<%# Container.DataItem %>')" class="history-link">
                                            <%# Container.DataItem %>
                                        </a>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <div class="information">
                            <h3 class="header">Weather Details</h3>
                            <div class="detailWeather">
                                <ul>
                                    <li class="climateInfo">
                                        <span>Location</span>
                                        <span>
                                         <asp:Label ID="LocationLabel2" runat="server" CssClass="location" Text=""></asp:Label>
                                        </span>
                                    </li>
                                    <li class="climateInfo">
                                        <span>Time</span>
                                        <span>
                                            <asp:Label ID="DateLabel2" runat="server" CssClass="time" Text=""></asp:Label>
                                        </span>
                                    </li>
                                    <li class="climateInfo">
                                        <span>Humidity</span>
                                        <span>
                                            <asp:Label ID="Humidity" runat="server" CssClass="location" Text=""></asp:Label>
                                        </span>
                                    </li>
                                    <li class="climateInfo">
                                        <span>Wind</span>
                                        <asp:Label ID="WindSpeed" runat="server" CssClass="location" Text=""></asp:Label>
                                    </li>
                                    <li class="climateInfo">
                                        <span>Feel Like</span>
                                        <span>
                                            <asp:Label ID="WindChill" runat="server" CssClass="location" Text=""></asp:Label>
                                           </span>
                                      </li>
                                </ul>
                            </div>
                        </div>
                        <asp:Button ID="btnNextDays" CssClass="next" runat="server" Text="Next Hours" OnClick="btnNextDays_Click" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
