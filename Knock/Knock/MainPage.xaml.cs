﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Text;
using Facebook;
using Microsoft.Phone.Shell;
using com.shephertz.app42.paas.sdk.windows;
using com.shephertz.app42.paas.sdk.windows.upload;
using com.shephertz.app42.paas.sdk.windows.storage;
using com.shephertz.app42.paas.sdk.windows.message;
using com.shephertz.app42.paas.sdk.windows.social;

namespace Knock
{
    public partial class MainPage : PhoneApplicationPage, App42Callback
    {
        private String _accessToken;
        private WebBrowser _webBrowser;
        public const String strConnectionString = @"isostore:/FbDB.sdf";
        String appId = "Your facebook app id";
        private const String ExtendedPermissions = "read_stream,publish_stream,offline_access,publish_actions";
        private readonly FacebookClient _fb = new FacebookClient();
       
        public  ProgressIndicator indicator = new ProgressIndicator

        {

            IsVisible = true,

            IsIndeterminate = true,

            Text = "Loading ...!"

        };
        public MainPage()
        {
            InitializeComponent();
            textBlock1.Visibility = Visibility.Collapsed;
            button1.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Get FacebookOAuthClient.
        /// </summary>
        void MainPage_Loaded()
        {
            using (DbStorage fbdb = new DbStorage(strConnectionString))
            {
                IQueryable<Db> fbQuery = from db in fbdb.user select db;
                Db ac = fbQuery.FirstOrDefault();
                if(ac == null){

                    var loginUrl = GetFacebookLoginUrl(appId, ExtendedPermissions);
                    //Add webBrowser to the contentPanel
                    _webBrowser.Navigate(loginUrl);
                    ContentPanel.Children.Add(_webBrowser);
                    _webBrowser.Navigated += WebBrowser_Navigated;
                    //Open the facebook login page into the browser
           
            }
        }
        }

        /// <summary>
        /// Get access token from facebook and save to local database.
        /// </summary>
        void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (!_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                MessageBox.Show("tt");
                return;
            }

            if (oauthResult.IsSuccess)
            {
                var accessToken1 = oauthResult.AccessToken;
                MessageBox.Show(accessToken1);
                _accessToken = oauthResult.AccessToken;
                //AccessToken is used when you want to use API as a user
                //This example is not using it at all just showing it in a messagebox

                //Adding data to the local database
                using (DbStorage fbdb = new DbStorage(strConnectionString))
                {
                    IQueryable<Db> fbQuery = from db in fbdb.user select db;
                    Db ac = fbQuery.FirstOrDefault();
                    if (ac == null)
                    {
                        Db newUser = new Db
                        {
                            AccessToken = _accessToken,
                        };

                        fbdb.user.InsertOnSubmit(newUser);
                        fbdb.SubmitChanges();
                    }

                }
                //Hide the browser controller
                _webBrowser.Visibility = System.Windows.Visibility.Collapsed;
                ContentPanel.Children.Remove(_webBrowser);

                SystemTray.SetProgressIndicator(this, indicator);
                Util myProfile = new Util(_accessToken);
                myProfile.MyProfile();

                // load friend list from App42
                GetFacebookFriendsFromSocial(_accessToken);
            }
            else
            {
                // user cancelled
                MessageBox.Show(oauthResult.ErrorDescription);
            }
            
        }

        // select friend from friend list..
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            FriendsList fl = listBox.SelectedItem as FriendsList;
            var Items = new App42Share(fl);
            //  open popup to share image with your selected friend.
            Items.display_cspopup();
        }

        // create database.
        private void CreateDbIfNotExists()
        {
            using (DbStorage fbDb = new DbStorage(strConnectionString))
            {
                if (fbDb.DatabaseExists() == false)
                {
                    fbDb.CreateDatabase();
                    image1.Visibility = Visibility.Collapsed;
                    LoginText.Visibility = Visibility.Collapsed;
                    textBlock1.Visibility = Visibility.Visible;
                    button1.Visibility = Visibility.Visible;
                    _webBrowser = new WebBrowser();
                    MainPage_Loaded();
                  
                }
                else
                {
                    using (DbStorage fbdb = new DbStorage(strConnectionString))
                    {
                        
                        IQueryable<Db> fbQuery = from db in fbdb.user select db;
                        Db ac = fbQuery.FirstOrDefault();
                        if (ac == null)
                        {
                            image1.Visibility = Visibility.Collapsed;
                            LoginText.Visibility = Visibility.Collapsed;
                            textBlock1.Visibility = Visibility.Visible;
                            button1.Visibility = Visibility.Visible;
                            _webBrowser = new WebBrowser();
                            MainPage_Loaded();
                        }
                        else
                        {
                            image1.Visibility = Visibility.Collapsed;
                            LoginText.Visibility = Visibility.Collapsed;
                            textBlock1.Visibility = Visibility.Visible;
                            button1.Visibility = Visibility.Visible;
                            SystemTray.SetProgressIndicator(this, indicator);
                            // load friend list from App42
                            GetFacebookFriendsFromSocial(ac.AccessToken.ToString());
                        }
                        
                        
                    }
                }
            }
        }

        // Login with facebook. 
        private void Login(object sender, MouseEventArgs e)
        {
            // create database.
            CreateDbIfNotExists();
        }

        public void GetFacebookFriendsFromSocial(String fbAccessToken) 
        {
            // Get Facebook Friends From App42 Social Service
            Util.socialService.GetFacebookFriendsOAuth(fbAccessToken,this);
        
        }

        // App42 callback when Object is successfully loded.
        void App42Callback.OnSuccess(object response)
        {
            // App42Callback if social
            if (response is Social)
            {
                Social social = (Social)response;
                // Get All friends from social object.
                IList<Social.Friends> fbList = social.GetFriendList();
                for (int i = 0; i < fbList.Count; i++)
                {
                    String id = fbList[i].GetId(); ;
                    String name = fbList[i].GetName();
                    String picture = fbList[i].GetPicture();
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        FriendsList row = new FriendsList();
                        row.userName.Text = name;
                        row.userId.Text = id;
                        row.myimage.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(picture);
                        // friend added in to list.
                        L1.Items.Add(row);
                    });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    indicator.IsVisible = false;
                });
            }
        }

        // App42 callback when exception
        void App42Callback.OnException(App42Exception exception)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(exception.Message);
            });
        }

        // Open Gallery.
        private void ShowGallery(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(String.Format("/SharedFiles.xaml" +
                                    "?Refresh=true&random={0}", Guid.NewGuid()), UriKind.Relative));
        }


        // create facebook login URI to authenticate.
        private Uri GetFacebookLoginUrl(String appId, String extendedPermissions)
        {
            var parameters = new Dictionary<String, object>();
            parameters["client_id"] = appId;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the 'scope' only if we have extendedPermissions.
            if (!String.IsNullOrEmpty(extendedPermissions))
            {
                // A comma-delimited list of permissions
                parameters["scope"] = extendedPermissions;
            }

            return _fb.GetLoginUrl(parameters);
        }
    }
}