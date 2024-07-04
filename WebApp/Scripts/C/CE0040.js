Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = '盤點管理作業';
    var p0_default = "";
    var T1GetExcel = '/api/CE0040/Excel';
    var T1F1 = '';//T1grid點選的盤點單號
    var mLabelWidth = 70;
    var mWidth = 230;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    //盤點年月
    var SetYmComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0040/GetSetymCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                if (DataCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                    p0_default = store.getAt(0).get('VALUE');
                    T1Form.getForm().findField("CHK_YM").setValue(p0_default);
                    GetSetAtime(p0_default);
                    GetChkEndtime(p0_default);
                    GetChkClosetime(p0_default);
                    T2Load();
                    T1Load();
                }
            }
        },
        autoLoad: true
    });
    //庫房代碼
    var SetWhNo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0040/GetWhNoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //盤點單開立完成時間
    function GetSetAtime(set_ym) {
        Ext.Ajax.request({
            url: '/api/CE0040/GetSetAtime',
            method: reqVal_p,
            params: {
                set_ym: set_ym
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('SetAtime').setValue(data.etts[0]);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    //盤點單結束日期
    function GetChkEndtime(set_ym) {
        Ext.Ajax.request({
            url: '/api/CE0040/GetChkEndtime',
            method: reqVal_p,
            params: {
                set_ym: set_ym
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('ChkEndtime').setValue(data.etts[0]);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    //全院盤點結束時間
    function GetChkClosetime(set_ym) {
        Ext.getCmp('SettChkClosetimeBtn').disable();
        Ext.Ajax.request({
            url: '/api/CE0040/GetChkClosetime',
            method: reqVal_p,
            params: {
                set_ym: set_ym
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length == 0) {
                        Ext.getCmp('SettChkClosetimeBtn').enable();
                    }
                    else if (data.etts[0] == null || data.etts[0] == "") {
                        Ext.getCmp('SettChkClosetimeBtn').enable();
                    }
                    T1Query.getForm().findField('ChkClosetime').setValue(data.etts[0]);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    //上方查詢區塊
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    bodyStyle: 'padding: 3px 15px;',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '盤點年月',
                            name: 'P0',
                            id: 'P0',
                            store: SetYmComboGet,
                            labelWidth: mLabelWidth,
                            fieldCls: 'required',
                            width: 180,
                            displayField: 'VALUE',
                            valueField: 'VALUE',
                            listeners: {
                                select: function (combo, records, eOpts) {
                                    T1Form.getForm().findField("CHK_YM").setValue(T1Query.getForm().findField('P0').getValue());
                                    GetSetAtime(T1Query.getForm().findField('P0').getValue());
                                    GetChkEndtime(T1Query.getForm().findField('P0').getValue());
                                    GetChkClosetime(T1Query.getForm().findField('P0').getValue());
                                }
                            }
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '庫房代碼',
                            name: 'P1',
                            id: 'P1',
                            store: SetWhNo,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'checkbox',
                            name: 'P2',
                            id: 'P2',
                            boxLabel: '盤點未完全',
                            checked: (menuLink == 'AA0216' ? true : false),
                            width:80,
                            padding: '0 4 0 10'
                        },
                        {
                            xtype: 'checkbox',
                            name: 'P3',
                            id: 'P3',
                            boxLabel: '完全未盤點',
                            checked: (menuLink=='AA0215'? true : false),
                            width: 80,
                            padding: '0 4 0 10',
                            hidden: ['AA0215', 'AA0216', 'AA0220'].includes(menuLink) ? false : true
                        },
                        {
                            xtype: 'checkbox',
                            name: 'P4',
                            id: 'P4',
                            boxLabel: '有盤點',
                            checked: (menuLink == 'AA0220' ? true : false),
                            width: 80,
                            padding: '0 4 0 10',
                            hidden: ['AA0215', 'AA0216', 'AA0220'].includes(menuLink) ? false : true
                        },
                        {
                            xtype: 'tbspacer',
                            width: ['AA0215', 'AA0216', 'AA0220'].includes(menuLink) ? 0 :160
                        },
                        {
                            xtype: 'button',
                            margin: '0 0 0 10',
                            text: '查詢',
                            handler: function () {
                                GetSetAtime(T1Query.getForm().findField('P0').getValue());
                                GetChkEndtime(T1Query.getForm().findField('P0').getValue());
                                GetChkClosetime(T1Query.getForm().findField('P0').getValue());
                                msglabel('訊息區:');
                                T1F1 = '';//重設選擇的盤點單號
                                T2Load();
                                T1Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                T1Query.getForm().findField('P0').setValue(p0_default);
                                T1Form.getForm().findField("CHK_YM").setValue(p0_default);
                                T1F1 = "";//重設選擇的盤點單號
                                T2Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '設定盤點結束日期',
                            id: '',
                            handler: function () {
                                SetChkEndTimeWindow.show();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '結束全院盤點',
                            disabled: true,
                            hidden:true,
                            id: 'SettChkClosetimeBtn',
                            handler: function () {
                                CheckChkClosetime();
                            }
                        }
                    ]
                }, {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    bodyStyle: 'padding: 3px 15px;',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'SetAtime',
                            id: 'SetAtime',
                            labelWidth: 140,
                            width: 250,
                            fieldLabel: '盤點單開立完成時間'
                        }, {
                            xtype: 'displayfield',
                            name: 'ChkEndtime',
                            id: 'ChkEndtime',
                            labelWidth: 140,
                            width: 250,
                            fieldLabel: '盤點結束日期'
                        }, {
                            xtype: 'displayfield',
                            name: 'ChkClosetime',
                            id: 'ChkClosetime',
                            labelWidth: 140,
                            width: 250,
                            hidden:true,
                            fieldLabel: '全院盤點結束時間'
                        }
                    ]
                }
            ]
        }]
    });

    //Master
    var T1LastRec = null;
    var T1Store = Ext.create('WEBAPP.store.CE0040M', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    menuLink: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        msglabel('查無資料!');
                    }
                }
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var param = new Array();
                            if (T2Store.getCount() > 0) {
                                param.push({ name: 'CHK_NO', value: T1F1 });
                                PostForm(T1GetExcel, param);
                                msglabel('訊息區:匯出完成');
                            }
                            else {
                                Ext.Msg.alert('訊息', '無資料可匯出');
                            }
                        }
                    });
                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "盤點年月",
                dataIndex: 'CHK_YM',
                width: 80
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 180
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 150
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLSNAME',
                width: 150
            },
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS',
                width: 150
            },
            {
                text: "總量",
                dataIndex: 'CHK_TOTAL',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "已盤點量",
                dataIndex: 'CHK_NUM',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;
                if (T1LastRec) {
                    T1F1 = T1LastRec.data.CHK_NO;
                    T2Load();
                }
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    //Detail
    var T2LastRec = null;
    var T2Store = Ext.create('WEBAPP.store.CE0040D', {
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T2Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#export').setDisabled(false);
                        T2LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                    }
                }
            }
        }
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 180
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "消耗量",
                dataIndex: 'USE_QTY',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "差異量",
                dataIndex: 'INVENTORY',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "差異金額",
                dataIndex: 'DIFF_AMOUNT',
                width: 120,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點時間",
                dataIndex: 'CHK_TIME',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ]
    });
    function T2Load() {
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }

    //設定盤點結束日期(小視窗)
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        height: 75,
        width: 300,
        fieldDefaults: {
            labelAlign: 'right'
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'CHK_YM'
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '盤點結束日期',
                            name: 'CHK_END_TIME',
                            labelWidth: 100,
                            width: 250,
                            padding: '0 4 0 20',
                            labelAlign: 'right'
                        }
                    ]
                }
            ]
        }]
    });
    var SetChkEndTimeWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T1Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "設定盤點結束日期",
        buttons: [
            {
                text: '確定',
                id: 'SetChkEndTimeConfirmBtn',
                handler: function () {
                    if (T1Form.getForm().findField('CHK_END_TIME').getValue() == null || T1Form.getForm().findField('CHK_END_TIME').getValue() == undefined) {
                        Ext.Msg.alert('提醒', '請選擇盤點結束日期');
                        return;
                    }
                    if (T1Form.getForm().findField('CHK_END_TIME').isValid() == false) {
                        Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                        return;
                    }
                    url = '/api/CE0040/SetChkEndTime';

                    var windowMask = new Ext.LoadMask(SetChkEndTimeWindow, { msg: '處理中...' });

                    myMask.show();
                    windowMask.show();
                    Ext.Ajax.request({
                        url: '/api/CE0040/SetChkEndTime',
                        method: reqVal_p,
                        params: {
                            chk_ym: T1Form.getForm().findField('CHK_YM').getValue(),
                            chk_endtime: getDateFormat(T1Form.getForm().findField('CHK_END_TIME').getValue())
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('盤點結束日期設定完成');
                                myMask.hide();
                                windowMask.hide();
                                GetChkEndtime(T1Form.getForm().findField('CHK_YM').getValue());
                            } else {
                                Ext.Msg.alert('失敗', '發生例外錯誤');
                                myMask.hide();
                                windowMask.hide();
                            }
                            T1Form.getForm().findField('CHK_END_TIME').setValue("");
                            SetChkEndTimeWindow.hide();
                        },
                        failure: function (response, options) {
                            myMask.hide();
                            windowMask.hide();
                            T1Form.getForm().findField('CHK_END_TIME').setValue("");
                            SetChkEndTimeWindow.hide();
                        }
                    });
                }
            },
            {
                text: '取消',
                handler: function () {
                    T1Form.getForm().findField('CHK_END_TIME').setValue("");
                    SetChkEndTimeWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                SetChkEndTimeWindow.center();
            }
        }
    });
    SetChkEndTimeWindow.hide();


    //檢查是否有庫房未盤點
    function CheckChkClosetime() {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0040/CheckChkClosetime',
            method: reqVal_p,
            params: {
                set_ym: T1Query.getForm().findField('P0').getValue()
            },
            timeout: 0,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.etts.length == 0) {
                    Ext.MessageBox.confirm('結束全院盤點', '是否確定結束 ' + T1Query.getForm().findField('P0').getValue() + ' 全院盤點？結束後不可再修正', function (btn, text) {
                        if (btn === 'yes') {
                            SetChkClosetime();
                        }
                        else {
                            myMask.hide();
                        }
                    });
                }
                else {//有尚未盤點的項目，不可結束全院盤點
                    ShowChkData();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }
    //顯示尚未盤點的項目
    function ShowChkData() {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0040/ShowChkData',
            method: reqVal_p,
            params: {
                set_ym: T1Query.getForm().findField('P0').getValue()
            },
            timeout: 0,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var msg_text = "";
                    for (var i = 0; i < data.etts.length; i++) {
                        msg_text += "庫房：" + data.etts[i].CHK_WH_NO + ",   院內碼：" + data.etts[i].MMCODE + "</br>藥材名稱：" + data.etts[i].MMNAME + "</br>";
                        msg_text += "---------------------------------</br>";
                    }
                    msg_text += "以上項目尚未完成盤點，不可結束全院盤點";
                    Ext.Msg.alert('注意', msg_text);
                    myMask.hide();
                } else {
                    msglabel('顯示尚未盤點的項目發生例外錯誤');
                    Ext.Msg.alert('失敗', '顯示尚未盤點的項目發生例外錯誤');
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }
    //結束全院盤點
    function SetChkClosetime() {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0040/SetChkClosetime',
            method: reqVal_p,
            params: {
                p0: T1Query.getForm().findField('P0').getValue()
            },
            timeout: 0,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('結束全院盤點完成');
                    Ext.Msg.alert('成功', '結束全院盤點完成');
                    myMask.hide();
                    GetChkClosetime(T1Query.getForm().findField('P0').getValue());
                } else {
                    msglabel('結束全院盤點發生例外錯誤');
                    Ext.Msg.alert('失敗', '結束全院盤點發生例外錯誤');
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '55%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '45%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });

    function getDateFormat(value) {
        var yyyy = value.getFullYear().toString();
        var m = value.getMonth() + 1;
        var d = value.getDate();
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();
        return yyyy + "-" + mm + "-" + dd;
    }
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});