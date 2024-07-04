/**
 * Pickup class
 * By Chilun
 * 21 Dec 2012
 */
// 2014-11-17: Chilun, 刪除pickup舊程式,且修改可拉動大小
// 2014-11-28 Chilun, 改良Pickup View

// PopupPickup class
// By Chilun
// 2014-07-14

var winP = null;
var pickupT = null;
var pickupF = null;

var pickupWin = [];

//顯示Pickup Form
function showPickup(viewport, form, pickup, title, width, height) {
    form.mask();
    pickupF = pickup;
    if (pickupF != pickupT) {
        //檢查formWin存在
        var bExist = false;
        if (pickupWin.length > 0) {
            var i;
            for (i = 0; i < pickupWin.length; i++) {
                if (pickupWin[i].form == pickup) {
                    winP = pickupWin[i].win;
                    bExist = true;
                    break;
                }
            }
        }
        if (!bExist) {
            winP = null;
            var winActWidth = width;
            var winActHeight = height;
            var maxWidth = viewport.width - 10;
            var maxHeight = viewport.height - 10;
            winP = Ext.widget('window', {
                title: title,
                layout: 'fit',
                autoScroll: false,
                constrain: true,
                resizable: true,
                modal: true,
                width: winActWidth, maxWidth: maxWidth,
                height: winActHeight, maxHeight: maxHeight,
                items: pickup,
                closeAction: 'hide',
                listeners: {
                    beforeclose: {
                        fn: function () {
                            hidePickup(form);
                        }
                    },
                    move: function (xwin, x, y, eOpts) {
                        xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                        xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                    },
                    resize: function (xwin, width, height) {
                        winActWidth = width;
                        winActHeight = height;
                    }
                }
            });
            pickupWin.push({
                'form': pickupF,
                'win': winP
            });
        }

        pickupT = pickup;
    }
    if (winP.hidden) {
        winP.show();
    }
    //return winP;
}

//隱藏Pickup Form
function hidePickup(form) {
    try {
        form.unmask();
        if (!winP.hidden) {
            if (pickupF != pickupT) {
                winP.close();
            }
            else {
                winP.hide();
            }
        }
    } catch (e) {
    }
}
//設定Pickup Title
function setPickupTitle(t) {
    try {
        winP.setTitle(t);
    } catch (e) {

    }
}
//設定Pickup Form Focus
function setPickupFocus(f) {
    try {
        winP.defaultFocus = f;
    } catch (e) {

    }
}

// 2014-11-28 Chilun, 改良Pickup View
var viewportPickup = null;
var parentformPickup = null;
var winPickup = null;
var widthPickup, heightPickup;
var pickupArray = [];
var lastrecPickup;

var titlePickup = '';
var geturlPickup = '';
var modelPickup = null;
var storePickup = null;
var queryPickup = null;
var gridPickup = null;
var sortItemPickup = null;
var queryItemPickup = null;
var gridItemPickup = null;

//檢查pickupArray有否存在
function checkPickupExist() {
    var bExist = false;
    if (pickupArray.length > 0) {
        for (i = 0; i < pickupArray.length; i++) {
            if (pickupArray[i].model == modelPickup) {
                winPickup = pickupArray[i].win;
                //modelPickup = pickupArray[i].model;
                storePickup = pickupArray[i].store;
                queryPickup = pickupArray[i].query;
                gridPickup = pickupArray[i].grid;
                bExist = true;
                break;
            }
        }
    }
    return bExist;
}

//設定Pickup View
function setPickup(storeparam, puttoform) {
    try {
        Ext.suspendLayouts();

        var title = titlePickup;
        var purl = geturlPickup;
        var model = modelPickup;
        var sortitem = sortItemPickup;
        var queryitem = queryItemPickup;
        var gridcolumn = gridItemPickup;
        var width = widthPickup;
        var height = heightPickup;
        var viewport = viewportPickup;
        var parentform = parentformPickup;

        var PStore = Ext.create('Ext.data.Store', {
            model: model,
            pageSize: 20,
            remoteSort: true,
            sorters: sortitem,
            listeners: {
                beforeload: function (store, options) {
                    var np = storeparam(PQuery);
                    Ext.apply(store.proxy.extraParams, np);
                }
            },
            proxy: {
                type: 'ajax',
                actionMethods: 'POST',
                url: purl,
                reader: {
                    type: 'json',
                    root: 'ds.T1',
                    totalProperty: 'ds.T1C[0].RC'
                }
            }
        });
        var PQuery = Ext.widget({
            xtype: 'form',
            layout: 'hbox',
            border: false,
            items: [{
                layout: 'hbox',
                border: false,
                items: queryitem
            }, {
                border: false,
                items: [
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            PStore.loadPage(1);
                        }
                    }, {
                        xtype: 'button',
                        text: '選取',
                        handler: function () {
                            putPickupToForm(PGrid, parentform, puttoform, lastrecPickup)
                        }
                    }
                ]
            }
            ]
        });
        var PTool = Ext.create('Ext.PagingToolbar', {
            store: PStore,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: []
        });
        var PForm = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            items: [PGrid]
        });
        var PGrid = Ext.create('Ext.grid.Panel', {
            title: '',
            store: PStore,
            plain: true,
            autoScroll: true,
            loadingText: '處理中...',
            loadMask: true,

            cls: 'P1',

            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '查詢',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: PQuery
            }, {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [PTool]
            }],
            columns: gridcolumn,
            listeners: {
                selectionchange: function (model, records) {
                    lastrecPickup = records[0];
                }
                , itemdblclick: function (dv, record, item, index, e) {
                    lastrecPickup = record;
                    putPickupToForm(PGrid, parentform, puttoform, record)
                }
            }
        });

        winPickup = null;
        var winActWidth = width;
        var winActHeight = height;
        var maxWidth = viewport.width - 10;
        var maxHeight = viewport.height - 10;
        winPickup = Ext.widget('window', {
            title: title,
            layout: 'fit',
            autoScroll: false,
            constrain: true,
            resizable: true,
            modal: true,
            width: winActWidth, maxWidth: maxWidth,
            height: winActHeight, maxHeight: maxHeight,
            items: {
                xtype: 'form',
                layout: 'fit',
                title: '',
                items: PGrid
            },
            closeAction: 'hide',
            listeners: {
                beforeclose: {
                    fn: function () {
                        hidePickupView(parentform);
                    }
                },
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
        pickupArray.push({
            'model': modelPickup,
            'store': PStore,
            'query': PQuery,
            'grid': PGrid,
            'win': winPickup
        });
        storePickup = PStore;
        queryPickup = PQuery;
        gridPickup = PGrid;

        Ext.resumeLayouts(true);

    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}
//顯示Pickup View
function openPickupView(execfunc) {
    try {
        if (winPickup.hidden) {
            execfunc(storePickup, queryPickup, gridPickup);
            storePickup.currentPage = 1;
            storePickup.load({
                callback: function (records, operation, success) {
                    if (winPickup.hidden) {
                        winPickup.show();
                    }
                }
            });
        }
    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}
//將Pickup選好的值帶入到表單中
function putPickupToForm(pgrid, pform, puttoform, plastrec) {
    try {
        if (pgrid) {
            if (plastrec) {
                puttoform(plastrec);
            }
        }
        hidePickupView(pform);
    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}
//隱藏Pickup View
function hidePickupView(form) {
    try {
        winPickup.hide();
        form.unmask();
    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}
//設定Pickup2 Title
function setPickupViewTitle(t) {
    try {
        winPickup.setTitle(t);
    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}
//設定Pickup2 Form Focus
function setPickupViewFocus(f) {
    try {
        winPickup.defaultFocus = f;
    } catch (e) {
        Ext.MessageBox.alert('警告', '錯誤訊息: ' + e.message);
    }
}