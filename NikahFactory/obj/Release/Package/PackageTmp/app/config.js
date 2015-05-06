var app = angular.module('app', ['ngRoute']);

app.config(function ($routeProvider) {
    $routeProvider
      .when('/',
      {
          templateUrl: "html/landing.html",
          controller: "LandingController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "landing";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/settings',
      {
          templateUrl: "html/settings.html",
          controller: "SettingsController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "settings";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/home',
      {
          templateUrl: "html/home.html",
          controller: "HomeController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "home";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/search',
      {
          templateUrl: "html/search.html",
          controller: "SearchController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "search";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/alerts',
      {
          templateUrl: "html/alerts.html",
          controller: "AlertsController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "alerts";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/messages',
      {
          templateUrl: "html/messages.html",
          controller: "MessagesController",
          resolve: {
              app: function ($q, $location, $rootScope) {
                  var defer = $q.defer();
                  var next = "messages";
                  checkRedirect(defer, $location, next, $rootScope);
                  return defer.promise;
              }
          }
      })
      .when('/verifyguardianemail/:token',
      {
          templateUrl: "html/landing.html",
          controller: "LandingController"
      })
      .otherwise({
          redirectTo: "/"
      });
});

var redirect = 0;

app.run(function ($rootScope, $location, $http, $timeout) {
    $rootScope.$on('$locationChangeStart', function (event) {
        if (redirect == 1) {
            $('body').hide();
            setTimeout(function () {
                $('body').show();
                redirect = 0;
            }, 500);
        }
    });
    $rootScope.ViewProfile = function (UserID) {
        $http.get('api/Account/ViewProfile?token=' + getAuthToken() + '&UserID=' + UserID).then(function (response) {
            $rootScope.Profile = {
                FirstName: response.data.FirstName,
                Heading: response.data.Heading,
                State: response.data.State,
                Country: response.data.Country,
                Age: response.data.Age,
                Bio: response.data.Bio
            };
            if (response.data.Gender == 'Sister')
                $rootScope.Profile.Image = 'images/female.png';
            else
                $rootScope.Profile.Image = 'images/male.png';
            $timeout(function () {
                $.pageslide({ direction: 'right', href: '#ViewProfile' });
            }, 1);
        }, function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
        });
    };
});


function checkRedirect(defer, $location, next, $rootScope) {
    var token = getAuthToken();
    if (token != null) {
        $.ajax({
            url: 'api/Account/ValidToken',
            data: { "token": token },
            success: function () {
                $.ajax({
                    url: 'api/Account/ActiveUser',
                    data: { "token": token },
                    success: function () {
                        $rootScope.Active = true;
                        if (next == "landing") {
                            $location.path("/home");
                            redirect = 1;
                        }
                        defer.resolve();
                    },
                    error: function () {
                        $rootScope.Active = false;
                        $location.path("/settings");
                        redirect = 1;
                        defer.resolve();
                    }
                });
            },
            error: function () {
                $location.path("/");
                redirect = 1;
                defer.resolve();
            }
        });
    }
    else {
        $location.path("/");
        redirect = 1;
        defer.resolve();
    }
}

function getAuthToken()
{
    return getCookie("NikahFactoryToken");
}

function saveAuthToken(token)
{
    setCookie("NikahFactoryToken", token, 1);
}

function setCookie(c_name,value,exdays)
{
    var exdate=new Date();
    exdate.setDate(exdate.getDate() + exdays);
    var c_value=escape(value) + ((exdays==null) ? "" : "; expires="+exdate.toUTCString());
    document.cookie=c_name + "=" + c_value;
}

function getCookie(c_name)
{
    var c_value = document.cookie;
    var c_start = c_value.indexOf(" " + c_name + "=");
    if (c_start == -1)
    {
        c_start = c_value.indexOf(c_name + "=");
    }
    if (c_start == -1)
    {
        c_value = null;
    }
    else
    {
        c_start = c_value.indexOf("=", c_start) + 1;
        var c_end = c_value.indexOf(";", c_start);
        if (c_end == -1)
        {
            c_end = c_value.length;
        }
        c_value = unescape(c_value.substring(c_start,c_end));
    }
    return c_value;
}

app.controller('VerifyGuardianEmailController', function ($scope, $routeParams) {
    console.log('routeparams: ' + $routeParams);
});