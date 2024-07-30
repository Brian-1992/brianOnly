Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var reportUrl = '/Report/F/FA0070.aspx';
    var T1GetExcel = '/api/FA0070/Excel';
    var T3GetExcel = '/api/FA0070/Excel3';
    var T4GetExcel = '/api/FA0070/Excel4';
    // var T1Get = '/api/FA0070/All'; // 查詢(改為於store定義)
    var T1Name = "通信中心庫存成本調整報表";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mLabelWidth = 70;
    var mWidth = 150;

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    // 查詢欄位
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
                    name: 'PT',
                    id: 'PT',
                    xtype: 'hidden'
                },{
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4',
                    value: new Date()
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '年月不可空白');
                        }
                        else {
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'button',
                    text: '匯出',
                    id: 'T1xls',
                    disabled: true,
                    handler: function () {
                        if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                            Ext.Msg.alert('提醒', '年月不可空白');
                            msglabel("年月不可空白");
                        }
                        else {
                            var p = new Array();
                            p.push({ name: 'FN', value: '通信中心庫存成本調整報表.xls' });
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                            p.push({ name: 'pt', value: T1Query.getForm().findField('PT').getValue() });
                            PostForm(T1GetExcel, p);
                            msglabel('匯出完成');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '列印',
                    id: 'T1btn1',
                    disabled: true,
                    handler: function () {
                        showReport();
                    }
                },
                {
                    xtype: 'button',
                    text: '單位消耗品項明細檔下載',
                    id: 'T1btn3',
                    //disabled: true,
                    handler: function () {
                        showWin3();
                    }
                },
                {
                    xtype: 'button',
                    text: '品項總消耗檔下載',
                    id: 'T1btn4',
                    //disabled: true,
                    handler: function () {
                        showWin4();
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'SEQ', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'SKEY', type: 'string' },
            { name: 'MAT_CLSNAME', type: 'string' },
            { name: 'INV_TYPE', type: 'string' },

            { name: 'P_AMT', type: 'string' },
            { name: 'IN_AMT', type: 'string' },
            { name: 'OUT_AMT', type: 'string' },
            { name: 'CHECK_P_AMT', type: 'string' },
            { name: 'CHECK_M_AMT', type: 'string' },

            { name: 'CHECK_AMT', type: 'string' },
            { name: 'INV_AMT', type: 'string' },
            { name: 'REP_TIME', type: 'string' },
            { name: 'D_AMT', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0070/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    pt: T1Query.getForm().findField('PT').getValue() 
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0,
                first: 0
            },
            callback: function (records) {
                if (records.length > 0) {
                    Ext.getCmp('T1btn1').setDisabled(false);
                    Ext.getCmp('T1xls').setDisabled(false);
                    T1Query.getForm().findField('PT').setValue(records[0].data.REP_TIME);
                }
                else {

                }
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
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
                text: "物料分類",
                dataIndex: 'MAT_CLSNAME',
                width: 100
            }, {
                text: "存量分類",
                dataIndex: 'INV_TYPE',
                width: 100
            }, {
                text: "期初存貨成本",
                dataIndex: 'P_AMT',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "進貨成本",
                dataIndex: 'IN_AMT',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "撥發成本",
                dataIndex: 'OUT_AMT',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "盤盈",
                dataIndex: 'CHECK_P_AMT',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                text: "盤虧",
                dataIndex: 'CHECK_M_AMT',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "小計",
                dataIndex: 'CHECK_AMT',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "調整後期末存貨成本",
                dataIndex: 'INV_AMT',
                style: 'text-align:left',
                width: 140, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "差異金額",
                dataIndex: 'D_AMT',
                style: 'text-align:left',
                width: 140, align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },{
            header: "",
            flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            }
        }
    });
    function showReport() {
        if (!win) {
            var p0 = T1Query.getForm().findField('P0').rawValue == null ? '' : T1Query.getForm().findField('P0').rawValue;
            var pt = T1Query.getForm().findField('PT').getValue() == null ? '' : T1Query.getForm().findField('PT').getValue();
            var qstring = '?p0=' + p0 + '&pt=' + pt;

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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


    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '起始日期',
                name: 'T3D0',
                id: 'T3D0',
                vtype: 'dateRange',
                dateRange: { end: 'T3D1' },
                padding: '0 4 0 4', value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '截止日期',
                name: 'T3D1',
                id: 'T3D1',
                vtype: 'dateRange',
                dateRange: { begin: 'T3D0' },
                padding: '0 4 0 4', value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T3Submit', text: '確定', handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '單位消耗品項明細檔.xls' });
                p.push({ name: 'd0', value: T3Form.getForm().findField('T3D0').rawValue });
                p.push({ name: 'd1', value: T3Form.getForm().findField('T3D1').rawValue });
                PostForm(T3GetExcel, p);
                msglabel('訊息區:匯出完成');
                hideWin3();
            }
        },
        {
            itemId: 'cancel', text: '離開', handler: hideWin3
        }
        ]
    });
    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '單位消耗品項明細檔',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }

    var T4Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'monthfield',
                fieldLabel: '起始年月',
                name: 'T4D0',
                id: 'T4D0',
                padding: '0 4 0 4',
                value: new Date()
            }, {
                xtype: 'monthfield',
                fieldLabel: '截止年月',
                name: 'T4D1',
                id: 'T4D1',
                padding: '0 4 0 4',
                value: new Date()
            }
        ],
        buttons: [{
            itemId: 'T4Submit', text: '確定', handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '品項總消耗檔.xls' });
                p.push({ name: 'd0', value: T4Form.getForm().findField('T4D0').rawValue });
                p.push({ name: 'd1', value: T4Form.getForm().findField('T4D1').rawValue });
                PostForm(T4GetExcel, p);
                msglabel('訊息區:匯出完成');
                hideWin4();
            }
        },
        {
            itemId: 'cancel', text: '離開', handler: hideWin4
        }
        ]
    });
    var win4;
    var winActWidth4 = 300;
    var winActHeight4 = 200;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '品項總消耗檔',
            closeAction: 'hide',
            width: winActWidth4,
            height: winActHeight4,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T4Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth4 > 0) ? winActWidth4 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight4 > 0) ? winActHeight4 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth4 = width;
                    winActHeight4 = height;
                }
            }
        });
    }

    function showWin4() {
        if (win4.hidden) {
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
        }
    }


    //view 
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
                        items: [T1Grid]
                    }
                ]
            }]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});
