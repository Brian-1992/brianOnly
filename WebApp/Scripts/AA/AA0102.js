Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '../../../api/AA0102/Excel';
    var reportUrl = '/Report/A/AA0102.aspx';
    var Radio1 = '../../../api/AA0102/GetRadio1';
    var Radio2 = '../../../api/AA0102/GetRadio2';
    var Radio3 = '../../../api/AA0102/GetRadio3';
    var Radio4 = '../../../api/AA0102/GetRadio4';

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();


    //var st_month = Ext.create('Ext.data.Store', {
    //    fields: ['VALUE', 'COMBITEM']
    //});

    ////RadioGroup
    //Ext.Ajax.request({
    //    url: Radio1,
    //    method: reqVal_p,
    //    success: function (response) {
    //        var data = Ext.decode(response.responseText);

    //        if (data.success) {
    //            Ext.getCmp('ra1').setBoxLabel('應盤點單位(' + data.msg+")");
    //        }
    //    },
    //    failure: function (response, options) {

    //    }
    //});

    ////RadioGroup
    //Ext.Ajax.request({
    //    url: Radio2,
    //    method: reqVal_p,
    //    success: function (response) {
    //        var data = Ext.decode(response.responseText);

    //        if (data.success) {
    //            Ext.getCmp('ra2').setBoxLabel('行政科室(' + data.msg + ")");
    //        }
    //    },
    //    failure: function (response, options) {

    //    }
    //});

    ////RadioGroup
    //Ext.Ajax.request({
    //    url: Radio3,
    //    method: reqVal_p,
    //    success: function (response) {
    //        var data = Ext.decode(response.responseText);

    //        if (data.success) {
    //            Ext.getCmp('ra3').setBoxLabel('財務獨立單位(' + data.msg + ")");
    //        }
    //    },
    //    failure: function (response, options) {

    //    }
    //});

    ////RadioGroup
    //Ext.Ajax.request({
    //    url: Radio4,
    //    method: reqVal_p,
    //    success: function (response) {
    //        var data = Ext.decode(response.responseText);

    //        if (data.success) {
    //            Ext.getCmp('ra4').setBoxLabel('全院(' + data.msg + ")");
    //        }
    //    },
    //    failure: function (response, options) {

    //    }
    //});


    // 查詢欄位
    var mLabelWidth = 40;
    var mWidth = 180;
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
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'D0',
                    id: 'D0',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getDefaultValue(),
                    width: 160
                },
                {
                    xtype: 'radiogroup',
                    fieldLabel: '單位類別',
                    labelWidth: 60,
                    width: 490,
                    name: 'SHOWDATA',
                    items: [
                        { boxLabel: '應盤點單位', id: 'ra1', width: 110, name: 'SHOWDATARB', inputValue: 1 },
                        { boxLabel: '行政科室', id: 'ra2', width: 110, name: 'SHOWDATARB', inputValue: 2 },
                        { boxLabel: '財務獨立單位', id: 'ra3', width: 130, name: 'SHOWDATARB', inputValue: 3 },
                        { boxLabel: '全院', id: 'ra4', width: 110, name: 'SHOWDATARB', inputValue: 4 }
                    ],
                    listeners:
                    {
                        change: function (rg, nVal, oVal, eOpts) {
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '查詢本月需手動新增盤點品項',
                    handler: function(){
                        chkNotExistsWindow.show();
                        T2Load();
                }
                }
            ],
        }],

    });



    var T1Store = Ext.create('WEBAPP.store.AA.AA0102', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    d0: T1Query.getForm().findField('D0').getValue(),
                    showdata: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue,
                };
                Ext.Ajax.timeout = 300000;
                Ext.Ajax.setTimeout(300000);
                Ext.override(Ext.data.proxy.Server, { timeout: Ext.Ajax.timeout });
                Ext.override(Ext.data.Connection, { timeout: Ext.Ajax.timeout });
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                    Ext.getCmp('print').setDisabled(false);
                } else {
                    msglabel('查無符合條件的資料!');
                    Ext.getCmp('export').setDisabled(true);
                    Ext.getCmp('print').setDisabled(true);
                }
            }
        }
    });
    function T1Load() {
        if (T1Query.getForm().findField('D0').getValue() != "" && T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {

            T1Tool.moveFirst();
            msglabel('訊息區:');
        } else {
            Ext.Msg.alert('訊息', '請先填好查詢欄位');
        }
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'export',
                name: 'export',
                text: '匯出', //T1Query
                handler: function () {
                    var p = new Array();

                    if ((T1Query.getForm().findField('D0').getValue() != "") && T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue != "") {
                        p.push({ name: 'FN', value: '完全未盤點單位查詢報表' });
                        p.push({ name: 'd0', value: T1Query.getForm().findField('D0').getRawValue() });
                        p.push({ name: 'showdata', value: T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue });

                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    } else {
                        Ext.Msg.alert('訊息', '請先填好查詢欄位');
                        msglabel('訊息區:');
                    }

                }
            },
            {
                id: 'print',
                name: 'print',
                text: '列印',
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        reportUrl = '/Report/A/AA0102.aspx';
                        showReport();
                    }
                    else
                        Ext.Msg.alert('訊息', '需要有資料');
                }
            },

        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?type=' + "AA0102"
                + '&APPTIME1=' + T1Query.getForm().findField('D0').getRawValue()
                + '&showdata=' + T1Query.getForm().findField('SHOWDATA').getChecked()[0].inputValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "單位代碼",
                dataIndex: 'INID',
                width: 100
            }, {
                text: "單位名稱",
                dataIndex: 'INID_NAME',
                width: 150
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 200
            }, {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE_NAME',
                width: 80
            }, {
                text: "物料分類",
                dataIndex: 'CHK_CLASS_NAME',
                width: 80
            }, {
                text: "盤點階段",
                dataIndex: 'CHK_LEVEL_NAME',
                width: 80
            }, {
                text: "庫房類別",
                dataIndex: 'WH_KIND_NAME',
                width: 80
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });


    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;
        yyyy = new Date().getFullYear() - 1911;
        
        m = new Date().getMonth() + 1;
        var mm = m >= 10 ? m.toString() : "0" + m.toString();

        return yyyy.toString() + mm;
    }

    function getInterval(i) {
        var yyyy = 0;
        var m = 0;

        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() - i + 1;

        if (m == 0 || m < 0) {   //因為從目前5月算起，的前5月是12月，但它跑出來是0
            yyyy = yyyy - 1;
            m = m + 12;
        }

        var mm = m >= 10 ? m.toString() : "0" + m.toString();

        return yyyy.toString() + mm;
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INID', type: 'string' },
            { name: 'INID_NAME', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'CHK_TYPE_NAME', type: 'string' },
            { name: 'MMCODE_COUNT', type: 'string' },
            { name: 'MMCODE_STRING', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'INID', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0102/GetChkNotExists',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                //// 載入前將查詢條件P0值代入參數
                //var np = {
                //    p0: T1Query.getForm().findField('P0').getValue()
                //};
                //Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                text: "單位代碼",
                dataIndex: 'INID',
                width: 100
            }, {
                text: "單位名稱",
                dataIndex: 'INID_NAME',
                width: 150
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 200
            }, {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE_NAME',
                width: 80
            }, {
                text: "數量",
                dataIndex: 'MMCODE_COUNT',
                width: 80
            }, {
                text: "院內碼清單",
                dataIndex: 'MMCODE_STRING',
                flex:1
            }
        ],
    });
    function T2Load() {
        T2Tool.moveFirst();
    }
    var chkNotExistsWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'chkNotExistsWindow',
        width: windowWidth,
        height: windowHeight,
        title:'需手動新增盤點品項',
        xtype: 'form',
        layout: 'form',
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            split: true,
            collapsible: false,
            border: false,
            items: [T2Grid]
        }],
        buttons: [
            {
                disabled: false,
                text: '關閉',
                handler: function () {
                    chkNotExistsWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                chkNotExistsWindow.center();
            }
        }
    });
    chkNotExistsWindow.hide();

    T1Query.getForm().findField('D0').focus();
    T1Query.getForm().findField('D0').setValue(getDefaultValue());
    T1Query.getForm().findField('SHOWDATA').setValue({ SHOWDATARB: 1 });   // 初始化RB
    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('print').setDisabled(true);

});