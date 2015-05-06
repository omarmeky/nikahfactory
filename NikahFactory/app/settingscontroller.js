app.controller('SettingsController', function ($scope, $rootScope, $location, $http) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.Logout = function () {
        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        $location.path('/');
    }
    $scope.getActivity = function () {
        if ($rootScope.Active)
            return "Active";
        else
            return "Inactive";
    };
});

app.controller('ChangePasswordController', function ($scope, $http, $location) {
    $scope.processForm = function () {
        $http.post('api/Account/ChangePassword?token=' + getAuthToken(), { OldPassword: $scope.OldPassword, NewPassword: $scope.NewPassword }).then(function (response) {
            bootbox.alert('Password changed successfully.');
            $scope.OldPassword = "";
            $scope.NewPassword = "";
        },
        function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
            else
                bootbox.alert(error.data.ExceptionMessage, function () {
                    $scope.OldPassword = "";
                    $scope.NewPassword = "";
                });
        });
    };
});

app.controller('UpdatePaymentController', function ($scope, $http, $location) {
    $scope.processForm = function () {
        $http.post('api/Account/UpdatePayment?token=' + getAuthToken(), { CreditCard: $scope.CreditCard, SecurityCode: $scope.SecurityCode, ExpirationMonth: $scope.ExpirationMonth, ExpirationYear: $scope.ExpirationYear }).then(function (response) {
            bootbox.alert('Payment method updated successfully.');
            $scope.CreditCard = "";
            $scope.SecurityCode = "";
            $scope.ExpirationMonth = "";
            $scope.ExpirationYear = "";
        },
        function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
            else
                bootbox.alert(error.data.ExceptionMessage, function () {
                    $scope.CreditCard = "";
                    $scope.SecurityCode = "";
                    $scope.ExpirationMonth = "";
                    $scope.ExpirationYear = "";
                });
        });
    };
});

app.controller('SubscriptionController', function ($scope, $http) {
    $http.get('api/Account/Paused?token=' + getAuthToken()).then(function (response) {
        var paused = response.data.Paused;
        if (!paused) {
            $scope.Paused = false;
            $scope.Message = "Pausing your subscription will deactivate your account and prevent future billing. This will not take effect until your next billing cycle.";
            $scope.Button = "Pause";
        }
        else {
            $scope.Paused = true;
            $scope.Message = "We miss you. Come back!";
            $scope.Button = "Unpause";
        }
    }, function (error) {
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
    $scope.togglePause = function () {
        $http.post('api/Account/TogglePause?token=' + getAuthToken()).then(function (response) {
            if ($scope.Paused) {
                $scope.Message = "Pausing your subscription will deactivate your account and prevent future billing. This will not take effect until your next billing cycle.";
                $scope.Button = "Pause";
                $scope.Paused = false;
            }
            else {
                $scope.Message = "We miss you. Come back!";
                $scope.Button = "Unpause";
                $scope.Paused = true;
            }
        }, function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
        });
    };
});