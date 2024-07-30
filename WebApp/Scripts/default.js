function msglabel(msg) {
    $('#msglabel-labelEl', top.document)[0].innerHTML = msg;
}

var SessionInfo;
var userId = '';            //Added by Chilun Ho 2013-06-14
var userName = '', userInid = '';
var clickCode = '';
var tabs;                   //Added by Chilun Ho 2014-10-28
Ext.ariaWarn = Ext.emptyFn;

Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var getFileName = function (fg) {
        window.open('../Download/Index/' + fg);
    };
    var T2Get = '../api/flow/process/TraOnlineDocGet';
    var todoCntGet = '../api/Menu/GetTodoCnt';
    var UrlHome = '../Form/Index/UR/UR1013';
    var UrlLogo2 = '../Images/TSGH_logo_W.png';//正式環境
    var Title = '藥品及衛材供應管理系統';
    var IsPageLoad = true;

    var MenuTreeStore = Ext.create('WEBAPP.store.MenuTree', {
        listeners: {
            beforeload: function () {
                if (IsPageLoad) { return false; }
                else { showTree(); }
            },
            load: function (store, records, successful, eOpts) {
                hideTree();
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        }
    });

    window.MenuStore = MenuTreeStore;

    var MenuTree = Ext.create('Ext.tree.Panel', {
        id: "navtree",
        title: '',
        header: false,
        collapsible: true,
        useArrows: true,
        rootVisible: false,
        store: MenuTreeStore,
        multiSelect: false,
        singleExpand: false,
        loadMask: true,
        tbar: [{
            iconCls: 'refresh',
            handler: function () {
                this.up('treepanel').store.load();
            }
        }, {
            iconCls: 'question',
            handler: function () {
                var tree = Ext.getCmp('navtree');
                var selectedNode = tree.getSelectionModel().getLastSelected();
                if (selectedNode) {
                    //var FP = "";
                    //var FN = "";
                    //var FT = "";

                    //Ext.Ajax.request({
                    //    url: T2Get,
                    //    params: {
                    //        CODE: selectedNode.data.id
                    //    },
                    //    method: 'POST',
                    //    success: function (response) {
                    //        responseText = Ext.decode(response.responseText);
                    //        if (responseText.success) {
                    //            if (responseText.ds.T.length !== 0) {
                    //                getFileName(responseText.ds.T[0].MFG);
                    //            }
                    //            else {
                    //                Ext.MessageBox.alert('提醒', 'E0003:檔案不存在!');
                    //            }
                    //        } else {
                    //            Ext.Msg.alert('錯誤', '下載線上說明文件失敗');
                    //        }
                    //    },
                    //    failure: function (response, options) {
                    //        Ext.Msg.alert('錯誤', '下載線上說明文件失敗');
                    //    }
                    //});
                } else {
                    Ext.Msg.alert('警告', '請先選擇左方功能樹節點');
                }
            }
        }, {
            text: '全部展開',
            handler: function () {
                if (!-[1,]) {
                    Ext.Msg.alert('警告', "IE8以下瀏覽器無法使用「全部展開」功能!建議使用IE9以上或Chrome、Firefox瀏覽器");
                } else {
                    var tree = Ext.getCmp('navtree');
                    tree.getEl().mask('展開中...');
                    var toolbar = this.up('toolbar');
                    toolbar.disable();

                    tree.expandAll(function () {
                        tree.getEl().unmask();
                        toolbar.enable();
                    });
                }
            }
        }, {
            text: '全部收合',
            handler: function () {
                var tree = Ext.getCmp('navtree');
                var toolbar = this.up('toolbar');
                toolbar.disable();

                tree.collapseAll(function () {
                    toolbar.enable();
                });
            }
        }],
        listeners: {
            itemclick: function (sender, record, item, index, e, eOpts) {
                if (record.data.url.length > 0) {
                    setForm(record);
                    clickCode = record.data.id;
                }
            }
        }
    });
    var tab_seq = 0;
    function setForm(r) {
        var t = r.data.text;
        msglabel('訊息區:');
        //addTab(tabs, t, r.data.url);
        setTab(tabs, t, r.data.url);
    }

    function addTab(tp, title, src) {
        //點擊左方功能表時，右方TabPanel新增一頁籤，如果功能已存在，則跳到該頁籤 2018.10.4 吉威
        if (title.indexOf(' ') > -1) title = title.substring(0, title.indexOf(' '));
        var target = tp.child('[title=' + title + ']');
        if (target !== null) {
            tp.setActiveTab(target);
        }
        else {
            var iframe1 = document.createElement("IFRAME");
            iframe1.id = "frame" + tab_seq;
            iframe1.frameBorder = 0;
            iframe1.src = src;
            iframe1.height = "100%";
            iframe1.width = "100%";
            var tabItem = {
                title: title,
                id: 'tab' + tab_seq,
                itemId: "tab" + tab_seq,
                html: '<iframe src="' + src + '" id="frame' + tab_seq + '" width="100%" height="100%" frameborder="0"></iframe>',
                closable: true
            };
            var newTab = tp.add(tabItem);
            tp.setActiveTab(newTab);
            tab_seq++;
        }
    }

    function setTab(tp, title, src) {
        menu.collapse(false);
        setTimeout(function () {
            tabs.child('#tabMain').tab.setText(title);
            $("#mainContent")[0].src = src;
            msglabel('訊息區:');
        }, 1000);
    };

    var MenuQuery = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 2,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 40
        },

        items: [{
            //fieldLabel: 'MenuItem',
            name: 'MenuName',
            enforceMaxLength: true,
            fieldStyle: 'text-transform:uppercase',
            maxLength: 20,
            width: 100,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        MenuSearch();
                    }
                }
            }
        }, {
            xtype: 'button',
            text: '查詢',
            //iconCls: 'TRASearch',
            handler: MenuSearch
        }, { xtype: 'tbspacer' }, {
            xtype: 'button',
            text: '清除',
            //iconCls: 'TRAClear',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('MenuName').focus();
            }
        }]
    });

    // create the Data Store
    var MenuGridStore = Ext.create('WEBAPP.store.MenuGrid', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    MenuName: MenuQuery.getForm().findField('MenuName').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function MenuSearch() {
        MenuGridStore.load();
    }

    var MenuGrid = Ext.create('Ext.grid.Panel', {
        store: MenuGridStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [MenuQuery]
        }
        ],
        // grid columns
        columns: [{
            text: "中文名稱(程式編碼)",
            dataIndex: 'text',
            flex: 1,
            sortable: false
        }],
        listeners: {
            itemclick: function (view, node, item, index, e) {
                var record = node.store.getAt(index);
                if (record.data.url.length > 0)
                    setForm(record);
            }
        }
    });

    var nav = Ext.widget('tabpanel', {
        plain: true,
        defaults: {
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            }
        },
        items: [{
            title: '階層顯示',
            items: MenuTree
        }, {
            title: '輸入查詢',
            items: MenuGrid,
            listeners: {
                activate: function (tab) {
                    setTimeout(function () {
                        MenuQuery.getForm().findField('MenuName').focus();
                    }, 1);
                }
            }
        }]
    });

    tabs = Ext.widget('tabpanel', {
        plain: true,
        resizeTabs: true,
        deferredRender: false,
        cls: 'tabpanel',
        defaults: {
            autoScroll: false
        },
        items: [{
            id: 'tabMain',
            title: '首頁',
            html: '<iframe src="' + UrlHome + '" id="mainContent" width="100%" height="100%" frameborder="0"></iframe>',
            iconCls: 'home',
            closable: false
        }],
        listeners: {
            //頁籤字體改變顏色 by 吉威 2018/10/19
            tabchange: function (tabPanel, newCard, oldCard, eOpts) {
                if (oldCard) {
                    oldCard.tab.btnInnerEl.setStyle('color', 'black');
                }
                if (newCard) {
                    newCard.tab.btnInnerEl.setStyle('color', 'brown');
                }
            }
        }
    });

    menu = Ext.create('Ext.panel.Panel', {
        region: 'west',
        layout: 'fit',
        collapsible: true,
        title: Title,
        cls: 'menuTitle',
        iconCls: 'sysmenu',
        split: true,
        width: 230,
        minWidth: 50,
        minHeight: 140,
        border: false,
        items: [nav]
    });

    var MsgForm = Ext.widget({
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        frame: false,
        height: '100',
        fieldDefaults: {
            labelWidth: 500,
            width: 500
        },

        items: [{
            xtype: 'displayfield',
            id: 'msglabel',
            fieldLabel: '訊息區',
            name: 'msglabel'
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        //suspendLayout: true,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            region: 'north',
            id: 'n',
            title: '',
            collapsible: false,
            split: false,
            height: 45,
            minHeight: 24,
            border: false,
            frame: false,
            items: [{
                border: false,
                layout: {
                    type: 'hbox'
                },
                defaults: {
                    border: false
                },
                items: [{
                    //items: logo,
                    id: 'n1',
                    html: '<image height="43px" width="129px" id="logoImage" src="' + UrlLogo2 + '" />&nbsp;&nbsp;&nbsp;&nbsp;',
                    height: 45
                }, {
                    id: 'n2',
                    height: 24,
                    html: '<table border="0" style="nowrap:nowrap;height: 100%;font-size:14px;color:#FF0000"><tr><td>' +
                        //'<marquee scrollamount="3" onMouseOver="this.stop()"; onMouseOut="this.start()"; style="width: 450px;font-size:15px;font-weight:normal;">歡迎使用本系統</marquee>' + '</td></tr></table>&nbsp;&nbsp;&nbsp;&nbsp;',
                        '&nbsp;&nbsp;&nbsp;&nbsp;</td></tr></table>&nbsp;&nbsp;&nbsp;&nbsp;',
                    flex: 1
                }, {
                    id: 'n3',
                    height: 24,
                    html: '<table border="0" style="nowrap:nowrap;height: 100%;color:#174F89">\
                                <tr>\
                                <td id="flow" style="width:120px;" nowrap></td>\
                                <td id="flow2" style="width:350px;" nowrap></td>\
                                <td id="status" nowrap>讀取中...</td>\
                                <td nowrap>您以&nbsp;</td>\
                                <td id="userName" style="text-decoration:underline" nowrap></td>\
                                <td nowrap>&nbsp;的身份登入|</td>\
                                <td nowrap><a href="#" id="loginStatus"></a>&nbsp;&nbsp;</td>\
                                </tr></table>'
                }]
            }]
        }, menu, /*{
            region: 'west',
            layout: 'fit',
            collapsible: true,
            title: Title,
            cls: 'menuTitle',
            iconCls: 'sysmenu',
            split: true,
            width: 230,
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [nav]
        },*/ {
            layout: {
                type: 'border',
                padding: 0
            },
            region: 'center',
            border: false,
            items: [{
                itemId: 'center-panel',
                region: 'center',
                layout: 'fit',
                border: false,
                title: '',
                items: [tabs]
            }
                ,
            {
                itemId: 'form',
                region: 'south',
                title: '',
                border: false,
                items: [MsgForm]
            }
            ]
        }]
    });

    /* 修改此函式需要加入使用者資訊，加入人員 612764_3410 Chilun Ho 2013-06-14 */
    var UserInfoStore = Ext.create('WEBAPP.store.UserInfo', {
        listeners: {
            load: function (sender, records, successful, eOpts) {
                if (successful) {
                    var r = records[0];
                    userId = r.get('TUSER');
                    userName = r.get('UNA');
                    userInid = r.get('INID');
                    userInidName = r.get('INID_NAME');

                    SessionInfo = {
                        USER_ID: userId,
                        USER_NAME: userName,
                        INID: userInid,
                        INID_NAME: userInidName
                    };

                    $('#userName')[0].innerHTML = userName;

                    if (userName === "") {
                        $("#loginStatus")[0].innerHTML = "登入";
                    } else {
                        $("#loginStatus")[0].innerHTML = "登出";
                    }
                }
                else {
                    $('#userName')[0].innerHTML = "";
                    $("#loginStatus")[0].innerHTML = "登入";
                }
                $('#status')[0].innerHTML = "";
                viewport.updateLayout();
            }
        }
    });

    Ext.get('loginStatus').on('click', function (e) {
        //        Ext.MessageBox.show({
        //            title: '登入',
        //            msg: '帳號:',
        //            width: 300,
        //            buttons: Ext.MessageBox.OKCANCEL,
        //            multiline: false,
        //            fn: showResultText,
        //            animateTarget: 'loginStatus'
        //        });
        if ($("#loginStatus")[0].innerHTML === "登入") {
            location.href = ".";
        }
        else {
            Ext.MessageBox.confirm('登出', '確定登出？', function (btn, text) {
                if (btn === 'yes') {
                    var UrlLogoff = '../Account/LogOff';

                    //使用一般Controller登出 2018.10.4 吉威
                    $('<form/>').attr('action', UrlLogoff)
                        .attr('method', 'POST')
                        .appendTo('body').submit().remove();
                    /*
                    Ext.Ajax.request({
                        url: UrlLogout,
                        method: 'POST',
                        success: function (response) {
                            $('#userName')[0].innerHTML = "";
                            //$("#loginStatus")[0].innerHTML = "登入";
                            //MenuTreeStore.load();
                            //$("#mainContent")[0].src = UrlHome;
                            //tabs.child('#tabMain').tab.setText('首頁');
                            location.href = "../Account/Login";
                        }
                    });*/
                }
            });
        }
    });
    function showResultText(btn, text) {
        $("#loginStatus")[0].innerHTML = "登出";
    }

    var treeMask = new Ext.LoadMask({
        msg: '處理中...',
        target: MenuTree
    });

    function hideTree() {
        treeMask.hide();
    }

    function showTree() {
        treeMask.show();
    }

    function returnMenuTree() {
        return MenuTreeStore;
    }

    //function loadToDoCounts() {
    //    return;
    //    //要更新代辦事項數量的程式碼都寫在這
    //    Ext.Ajax.request({
    //        url: todoCntGet,
    //        params: { ekgrp: userEkgrp, userid: userId },
    //        method: 'POST',
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                var tb_menuCnt = data.etts;
    //                if (tb_menuCnt.length > 0) {
    //                    loadToDoCount('MF04', tb_menuCnt[0].MF04);
    //                    loadToDoCount('MF02', tb_menuCnt[0].MF02);
    //                    loadToDoCount('SMenuName2', tb_menuCnt[0].SMenuName2);
    //                    loadToDoCount('NM02', tb_menuCnt[0].NM02);
    //                    loadToDoCount('PR01', tb_menuCnt[0].PR01);
    //                    loadToDoCount('RQ01', tb_menuCnt[0].RQ01);
    //                    loadToDoCount('NEGO_PRICE', tb_menuCnt[0].NEGO_PRICE);
    //                    loadToDoCount('NEGO_PRICE_V', tb_menuCnt[0].NEGO_PRICE_V);
    //                    loadToDoCount('QUOTO', tb_menuCnt[0].QUOTO);
    //                    loadToDoCount('CLOSE_QUOTO', tb_menuCnt[0].CLOSE_QUOTO);
    //                    loadToDoCount('QuotoQA_B', tb_menuCnt[0].QuotoQA_B);
    //                }
    //            }
    //        },
    //        failure: function (response, options) {
    //        }
    //    });
    //}

    IsPageLoad = false;
    UserInfoStore.load();
    MenuTreeStore.load({
        params: {},
        callback: function () {
            //一進到首頁，先更新一次代辦事項數量
            //setTimeout(function () {
            //    loadToDoCounts();
            //}, 2000);
            //loadToDoCounts();
            var firstNode = MenuTree.getRootNode();
            if (firstNode) MenuTree.expandNode(firstNode);
        }
    });

    //每分鐘更新一次代辦事項數量
    //setInterval(function () {
    //    loadToDoCounts();
    //}, 60000);
});