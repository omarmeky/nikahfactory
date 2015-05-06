app.controller('MessagesController', function ($scope, $location, $http, $timeout) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.Logout = function () {
        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        $location.path('/');
    }
    $scope.setActiveConversation = function(Conversation){
        $scope.ActiveConversation = Conversation;
        for (var i = 0; i < $scope.Conversations.length; i++)
            $scope.Conversations[i].Selected = false;
        $timeout(function(){
            Conversation.Selected = true;
        }, 1);
    }
    $scope.Send = function () {
        if ($scope.NewMessage) {
            $http.post('api/Account/Reply?token=' + getAuthToken(), { Receiver: $scope.ActiveConversation.UserID, ConversationId: $scope.ActiveConversation.ConversationID, Message: $scope.NewMessage }).then(function (response) {
                $scope.ActiveConversation.Messages.push(response.data);
                setTimeout(function () {
                    $(".chat-messages-list .content").slimScroll({ scrollBy: '10000px' });
                }, 100);
                $scope.NewMessage = "";
            }, function (error) {
                if (error.status == 403)
                    bootbox.alert(error.data.ExceptionMessage, function () {
                        document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                        $location.path('/');
                    });
            });
        }
    }
    $http.get('api/Account/Conversations?token=' + getAuthToken()).then(function (response) {
        if (response.data.length > 0) {
            $scope.Conversations = response.data;
            $scope.ActiveConversation = $scope.Conversations[0];
            $scope.Conversations[0].Selected = true;
            $http.post('api/Account/ViewMessages?token=' + getAuthToken());
        }
        else
            $scope.Conversations = [];
    }, function (error) {
        $scope.Conversations = [];
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
});

app.controller('NewMessagesController', function ($scope, $rootScope, $http) {
    $scope.NewMessages = [];
    $http.get('api/Account/NewMessages?token=' + getAuthToken()).then(function (response) {
        for (var i = 0; i < response.data.length; i++) {
            $scope.NewMessages.push({
                index: i,
                sender: response.data[i]
            });
        }
    }, function (error) {
        if (error.status == 403)
            bootbox.alert(error.data.ExceptionMessage, function () {
                document.cookie = 'NikahFactoryToken=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
                $location.path('/');
            });
    });
});