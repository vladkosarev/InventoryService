var app = angular.module("InventoryServiceApp", [
    "ngRoute", "jsonFormatter"
]);

app.config([
    "$routeProvider", function ($routeProvider) {
        //$routeProvider
        //    // Home
        //    .when("/", {
        //        templateUrl: "partials/home.html" /*,controller: ""*/
        //    })
        //    // else 404
        //    .otherwise("/404", { templateUrl: "partials/404.html", controller: "PageCtrl" });
    }
]);
angular.module("InventoryServiceApp").factory("endpoints", function () {
    return {
        hub: "/signalr",
        webApi: "/api/values/ActorSystemStates"
    };
});
angular.module("InventoryServiceApp").service("service", function ($q, $http, $rootScope) {
    this.POST = function (url, item) {
        var deferred = $q.defer();
        var load = JSON.stringify(item);
        $http.post(url, load, {
            headers: {
                'Content-Type': "application/json"
            }
        }).
            success(deferred.resolve).
            error(deferred.reject);
        $rootScope.allCurrentHttpPromises.push(deferred.promise);
        return deferred.promise;
    };
    this.GET = function (url) {
        var deferred = $q.defer();
        $http({
            method: "GET",
            url: url
        }).
            success(deferred.resolve).
            error(deferred.reject);
        $rootScope.allCurrentHttpPromises.push(deferred.promise);
        return deferred.promise;
    };
});
angular.module("InventoryServiceApp").factory("hub", function (endpoints, $timeout) {
    $.connection.hub.url = endpoints.hub;
    var inventoryServiceHub = $.connection.inventoryServiceHub;
    return {
        ready: function (f) {
            $.connection.hub.start().done(function () {
                var arg = arguments;
                $timeout(function () {
                    f && f.apply(null, arg);
                });
            });
        },
        inventoryServiceHub: inventoryServiceHub,
        server: inventoryServiceHub.server,
        client: function (name, f) {
            inventoryServiceHub.client[name] = function (response) {
                var arg = arguments;
                $timeout(function () {
                    f && f.apply(null, arg);
                });
            };
        }
    };
});

angular.module("InventoryServiceApp").controller("ActorsCtrl", function ($scope, $rootScope, $http, $q, $timeout, hub) {
    var messageCount = 0;

    $scope.performOperation = function (operationName,id, quantity,messageQty) {
        hub.server.performOperation(operationName, id, quantity, messageQty);
    };

    $scope.messageQuantity = 1;
    $scope.current = {};
    $scope . setCurrent = function(inv) {
        $scope.current = inv;
    };
    $scope.model = {};
    $scope.model.logMessages = true;
    var storage = function(a,b) {
        if (typeof (Storage) !== "undefined") {
            if (b) {
                localStorage.setItem(a, b);
                return undefined;
            } else {
             return   localStorage.getItem(a);
            }
        } else {
            console.log("Sorry! No Web Storage support..");
            return undefined;
        }
    };
    var initStorage = function() {
        $scope.model.realtime=  storage("model.realtime");
        $scope.model.logMessages = false;// storage("model.logMessages") ;
    };
    $scope.updateStorage=function () {
        storage("model.realtime", $scope.model.realtime);
        storage("model.logMessages", $scope.model.logMessages);
    }
    initStorage();
    var hasLoaded = false;
    var lastResponse = {};
    var lastResponseDict = {};
    $scope.realTimeInventories = [];

    var line1 = new TimeSeries();
    var smoothie = new SmoothieChart({ grid: {
        strokeStyle: 'rgb(125, 0, 0)',
        fillStyle: 'rgb(60, 0, 0)',
        lineWidth: 1,
        millisPerLine: 250,
        verticalSections: 6
    } });
    smoothie.addTimeSeries(line1);
   smoothie.streamTo(document.getElementById("mycanvas"), 1000 /*delay*/);
    $scope. updateGrid = function () {
        $scope.newUpdateAvailable = 0;

        $scope.realTimeInventories = $.map(lastResponseDict, function (value, index) {
            return [value];
        });
        //$("#jsGrid1")
        //    .jsGrid({
        //        width: "100%",
        //        height: "500px",
        //        inserting: false,
        //        editing: false,
        //        sorting: true,
        //        paging: false,

        //        data: lastResponse.RealTimeInventories,

        //        fields: [
        //            { name: "ProductId", type: "text", width: 200 },
        //            { name: "Quantity", type: "text", width: 200 },
        //            { name: "Reserved", type: "text", width: 200 },
        //            { name: "Holds", type: "text", width: 200 }
        //        ]
        //    });
    }
    $scope.sortType = 'ProductId'; // set the default sort type
    $scope.sortReverse = false;  // set the default sort order
    $scope.searchFish = '';     // set the default search/filter term
    $scope.newUpdateAvailable = 0;

    $scope.operationNames = [];

    hub.client("operationNames", function(operationNames) {
        $scope.operationNames = operationNames;
    });
    hub.client("inventoryData", function (response) {
        if (!response) return;
         console.log("PeakMessageSpeed: " + response.PeakMessageSpeed + "m/s   Speed: " + response.Speed+"m/s");
        hasLoaded = false;
        if (response.RealTimeInventories&& response.RealTimeInventories.length>1) {
            console.log("Got inventory " );
        }
        for (var i = 0; i < response.RealTimeInventories.length; i++) {
            var newInventory = response.RealTimeInventories[i];
            var productId = newInventory.ProductId;
            var cachedInv = lastResponseDict[productId];
            if (cachedInv) {
                if ((cachedInv.Quantity !== newInventory.Quantity) || (cachedInv.Reserved !== newInventory.Reserved) || (cachedInv.Holds !== newInventory.Holds)) {
                    $scope.newUpdateAvailable++;
                }
            } else {
                $scope.newUpdateAvailable++;
            }
            lastResponseDict[productId] = newInventory;
        }
        lastResponse = response;
        hasLoaded = true;
        if ($scope.model.realtime ) {
            $timeout(function () {
                if (hasLoaded) {
                   $scope.updateGrid();
                   hasLoaded = true;
                }
             },10);
        }
    });

    $scope.serverNotificationMessages = "";
    $scope.jsonNotificationMessages = "";
    $scope.messageSpeed = "";
    $scope.model.incomingMessages = [];
    $scope.exportInventory = function() {
        hub.server.backUpInventories();
    };

    $scope.downLoadInventoryExport = function() {
        
        var d = new Date();
        var filename = "real-time-inventory-export-" + d.getFullYear() + "-" + d.getMonth() + "-" + d.getDay() + "_" + d.getHours() + "-" + d.getMinutes() + "-" + d.getSeconds() + ".csv";
        var data = $scope.inventoryExport;
        var blob = new Blob([data], { type: 'text/csv' });
        if (window.navigator.msSaveOrOpenBlob) {
            window.navigator.msSaveBlob(blob, filename);
        }
        else {
            var elem = window.document.createElement('a');
            elem.href = window.URL.createObjectURL(blob);
            elem.download = filename;
            document.body.appendChild(elem);
            elem.click();
            document.body.removeChild(elem);
        }
        $scope.inventoryExport = false;
    };
    hub.client("inventoryExportCsv",
       function (response) {
           $scope.inventoryExport = response;
          

       });



    hub.client("jsonNotificationMessages",
       function (response) {
           $scope.jsonNotificationMessages = response;
       });

    hub.client("serverNotificationMessages",
        function(response) {
            $scope.serverNotificationMessages = response;
        });
    $scope.peekMessageCount = 0;
    hub.client("messageSpeed",
        function(response) {
            var speed = parseInt(response, 10);
            $scope.messageSpeed = speed;
            line1.append(new Date().getTime(), speed);
            if ($scope.peekMessageCount < speed) {
                $scope.peekMessageCount = speed;
            }
        });
    $scope.maxMessageCount = 15;
    hub.client("incomingMessage",
        function (response) {
            messageCount++;
            if (!$scope.model.logMessages) {
                return;
            }
            $timeout(function() {
                $scope.model.incomingMessagesFirst =messageCount+" : "+ response;
                    if ($scope.model.logMessages) {
                    } else {
                        $scope.model.incomingMessages = [];
                    }
                    $timeout((function (incomingMessagesFirst) {
                    return function () {
                     incomingMessagesFirst && $scope.model.incomingMessages.unshift(incomingMessagesFirst);
                        $scope.model.incomingMessagesFirst = false;
                        if ($scope.model.incomingMessages.length > $scope.maxMessageCount) {
                            while ($scope.model.incomingMessages.length > $scope.maxMessageCount) {
                                $scope.model.incomingMessages.pop();
                            }
                        }
                    }
                })($scope.model.incomingMessagesFirst), 500);
            });
        });

    hub.ready(function () {
        hub.server.getInventoryList();
    });
});