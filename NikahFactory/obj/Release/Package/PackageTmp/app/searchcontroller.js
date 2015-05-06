app.controller('SearchController', function ($scope, $location, $http) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.Logout = function () {
        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        $location.path('/');
    }
    $scope.MinAge = 20;
    $scope.MaxAge = 30;
    $scope.Countries = [];
    $scope.Submit = false;
    $scope.NoResults = false;
	
    $("#slider-range").css('width', '100%').slider({
        range: true,
        min: 16,
        max: 70,
        values: [20, 30],
		
        slide: function (event, ui) {
            $scope.MinAge = parseInt(ui.values[0]);
            $scope.MaxAge = parseInt(ui.values[1]);
            $scope.$apply();
        }
    });
    $scope.Search = function () {
        $scope.Submit = true;
        if ($scope.Countries.length > 0)
            $http.post('api/Account/Search?token=' + getAuthToken(), { Countries: $scope.Countries, MinAge: $scope.MinAge, MaxAge: $scope.MaxAge }).then(function (response) {
                if (response.data.length == 0) {
                    $scope.NoResults = true;
                    $scope.Results = [];
                }
                else {
                    $scope.NoResults = false;
                    $scope.Results = response.data;
                }
            }, function (error) {
                if (error.status == 403)
                    bootbox.alert(error.data.ExceptionMessage, function () {
                        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                        $location.path('/');
                    });
            });
    };
    $scope.Alert = function (Result) {
        $http.post('api/Account/Alert?token=' + getAuthToken() + '&UserID=' + parseInt(Result.UserID)).then(function (response) {
            Result.Alerted = true;
        }, function (error) {
            if (error.status == 403)
                bootbox.alert(error.data.ExceptionMessage, function () {
                    document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                    $location.path('/');
                });
        });
    };
    $scope.Message = function (UserID) {
        var message = "";
        bootbox.prompt("New Message", function (result) {
            $(".input-block-level").changeElementType("input");
            $(".input-block-level").css("height", "30px");
            $(".input-block-level").bind("propertychange keyup input paste", function (e) {});
            $('.modal-footer .btn-primary').html('Ok');
            if (message != "") {
                $http.post('api/Account/SendMessage?token=' + getAuthToken(), {UserID:UserID,Message:message}).then(function (response) {
                    bootbox.alert('Your message has been sent!');
                }, function (error) {
                    if (error.status == 403)
                        bootbox.alert(error.data.ExceptionMessage, function () {
                            document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                            $location.path('/');
                        });
                });
            }
        });
        $(".input-block-level").changeElementType("textarea");
        $(".input-block-level").css("height", "300px");
        $(".input-block-level").bind("propertychange keyup input paste", function (e) {
            message = $(".input-block-level").val();
        });
        $('.modal-footer .btn-primary').html('Send');
    }
});

(function ($) {
    $.fn.changeElementType = function (newType) {
        var attrs = {};
        $.each(this[0].attributes, function (idx, attr) {
            attrs[attr.nodeName] = attr.nodeValue;
        });
        this.replaceWith(function () {
            return $("<" + newType + "/>", attrs).append($(this).contents());
        });
    }
})(jQuery);