Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var reportUrl = '/Report/A/AA0132.aspx';
    var T1GetExcel = '/api/AA0132/Excel';
    // var T1Get = '/api/AA0132/All'; // 查詢(改為於store定義)
    var T1Name = "藥庫平時查詢";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mLabelWidth = 50;
    var mWidth = 160;

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
                    xtype: 'displayfield',
                    fieldLabel: '庫房',
                    labelAlign: 'right',
                    name: 'PP',
                    labelWidth: 50,
                    width: 100,
                    readOnly: true
                }, {
                    xtype: 'radiogroup',
                    fieldLabel: '類別',
                    name: 'Type',
                    width: 480,
                    items: [
                        { boxLabel: '口服', width: 100, name: 'Type', inputValue: '1', checked: true },
                        { boxLabel: '非口服', width: 100, name: 'Type', inputValue: '2' },
                        { boxLabel: '1~3級管制', width: 100, name: 'Type', inputValue: '3' },
                        { boxLabel: '4級管制', width: 100, name: 'Type', inputValue: '4' }
                    ]
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                        T1Query.getForm().findField('PP').setValue("PH1S");
                    }
                },
                {
                    xtype: 'button',
                    text: '匯出',
                    id: 'T1xls',
                    disabled: true,
                    handler: function () {
                            var p = new Array();
                            p.push({ name: 'FN', value: '藥庫平時查詢.xls' });
                            p.push({ name: 'p0', value: T1Query.getForm().findField('Type').getValue()['Type'] });
                            PostForm(T1GetExcel, p);
                            msglabel('匯出完成');
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
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'PMN_INVQTY', type: 'string' },
            { name: 'apl_inqty', type: 'string' },
            { name: 'apl_outqty', type: 'string' },

            { name: 'STORE_QTY', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'store_amount', type: 'string' },
            { name: 'STORE_QTYM', type: 'string' },
            { name: 'MIL_PRICE', type: 'string' },

            { name: 'EXG_INQTY', type: 'string' },
            { name: 'EXG_OUTQTY', type: 'string' },
            { name: 'CaddM', type: 'string' },
            { name: 'chk_qty', type: 'string' },
            { name: 'pack_qty', type: 'string' },

            { name: 'CHK_REMARK', type: 'string' },

            { name: 'STORE_QTYC', type: 'string' },
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0132/AllM',
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
                    p0: T1Query.getForm().findField('Type').getValue()['Type']
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            },
            callback: function (records) {
                
                if (records.length > 0) {
                    Ext.getCmp('T1btn1').setDisabled(false);
                    Ext.getCmp('T1xls').setDisabled(false);
                }
                else {
                    Ext.Msg.alert('提醒', ' 查無符合條件的資料！');
                    msglabel(' 查無符合條件的資料！');
                    Ext.getCmp('T1btn1').setDisabled(true);
                    Ext.getCmp('T1xls').setDisabled(true);
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
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 300
        }, {
            text: "上月結存",
            dataIndex: 'PMN_INVQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        },
        {
            text: '    民    品',
            columns: [
                {
                    text: "本月進貨",
                    dataIndex: 'apl_inqty',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }, {
                    text: "本月撥發",
                    dataIndex: 'apl_outqty',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }, {
                    text: "本月結存",
                    dataIndex: 'STORE_QTYC',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }, {
                    text: "單價",
                    dataIndex: 'AVG_PRICE',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }]
        },
        {
            text: '  軍  品',
            columns: [
                {
                    text: "數量",
                    dataIndex: 'STORE_QTYM',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }, {
                    text: "單價",
                    dataIndex: 'MIL_PRICE',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }]
        }, {
            text: '  調撥量',
            columns: [
                {
                    text: "入庫",
                    dataIndex: 'EXG_INQTY',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }, {
                    text: "出庫",
                    dataIndex: 'EXG_OUTQTY',
                    style: 'text-align:left',
                    width: 80, align: 'right'
                }
            ]
        }, {
            text: "軍+民",
            dataIndex: 'CaddM',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "盤點量",
            dataIndex: 'chk_qty',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "包裝量",
            dataIndex: 'pack_qty',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "備註",
            dataIndex: 'CHK_REMARK',
            width: 200
        }, {
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
            var p0 = T1Query.getForm().findField('Type').getValue()['Type'];

            var qstring = '?p0=' + p0;

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
    T1Query.getForm().findField('Type').focus();
    T1Query.getForm().findField('PP').setValue("PH1S");
});
