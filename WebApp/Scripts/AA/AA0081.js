Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var reportUrl = '/Report/A/AA0081.aspx';
    var T1GetExcel = '/api/AA0081/Excel';
    // var T1Get = '/api/AA0081/All'; // 查詢(改為於store定義)
    var T1Name = "藥庫月盤點查詢";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mLabelWidth = 70;
    var mWidth = 150;

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    var getTodayMonth = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        m = m.length > 1 ? m : "0" + m;
        return y + m ;
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
                    xtype: 'textfield',
                    fieldLabel: '庫房',
                    labelAlign: 'right',
                    name: 'PP',
                    labelWidth: 50,
                    width: 100,
                    readOnly: true
                },
                {
                    xtype: 'hidden',
                    name: 'P0',
                    id: 'P0',
                },
                {
                    xtype: 'radiogroup',
                    //fieldLabel: '年月/日期',
                    name: 'P0_radio',
                    id: 'P0_radio',
                    //fieldCls: 'required',
                    //labelWidth: mLabelWidth,
                    //width: mWidth,
                    //padding: '0 4 0 4',
                    margin: '0 4 0 30',
                    items: [
                        
                        {
                            boxLabel: '年月',
                            //labelWidth: 20,
                            width: 45,
                            checked: true,
                            inputValue: '0'
                        },
                        {
                            boxLabel: '日期',
                            //labelWidth: 20,
                            width: 45,
                            inputValue: '1'
                        },
                        
                    ],
                    listeners: {                        
                        change: function (radiogroup, newValue, oldValue) { 
                            
                            switch (newValue.P0_radio) {
                                case '0':   //選擇年月   
                                        T1Query.getForm().findField('P0_month').reset();
                                        T1Query.getForm().findField('P0_month').show();
                                        //T1Query.getForm().findField('P0_month').enable();
                                        T1Query.getForm().findField('P0_date').hide();
                                        //T1Query.getForm().findField('P0_date').disable();
                                        T1Query.getForm().findField('P0').setValue(T1Query.getForm().findField('P0_month').rawValue);
                                        break;        
                                case '1':   //選擇日期
                                        T1Query.getForm().findField('P0_month').hide();
                                        //T1Query.getForm().findField('P0_month').disable();
                                        T1Query.getForm().findField('P0_date').reset();
                                        T1Query.getForm().findField('P0_date').show();
                                        //T1Query.getForm().findField('P0_date').enable();
                                        T1Query.getForm().findField('P0').setValue(T1Query.getForm().findField('P0_date').rawValue);
                                        break;
                            }
                        }
                    }
                },
                {
                    xtype: 'monthfield',
                    name: 'P0_month',
                    labelWidth: 20,
                    width: 80,
                    fieldCls: 'required',
                    hidden: false,
                    value: getTodayMonth(),
                    listeners: {
                        change: function () {
                            T1Query.getForm().findField('P0').setValue(T1Query.getForm().findField('P0_month').rawValue);
                        }
                    }
                },
                {
                    xtype: 'datefield',
                    name: 'P0_date',
                    labelWidth: 20,
                    width: 80,
                    fieldCls: 'required',
                    hidden: true,
                    value: getTodayDate(),
                    listeners: {
                        change: function () {
                            T1Query.getForm().findField('P0').setValue(T1Query.getForm().findField('P0_date').rawValue);
                        }
                    }
                },
                {
                    xtype: 'radiogroup',
                    fieldLabel: '類別',
                    name: 'Type',
                    width: 480,
                    margin: '0 4 0 100',
                    items: [
                        { boxLabel: '口服', width: 100, name: 'Type', inputValue: '1', checked: true },
                        { boxLabel: '非口服', width: 100, name: 'Type', inputValue: '2' },
                        { boxLabel: '1~3級管制', width: 100, name: 'Type', inputValue: '3' },
                        { boxLabel: '4級管制', width: 100, name: 'Type', inputValue: '4' }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '年月/日期不可空白');
                        }
                        else {

                            T1Load();
                            msglabel('訊息區:');
                        }
                    }
                },
                {
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
                        if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                            Ext.Msg.alert('提醒', '年月/日期不可空白');
                            msglabel("年月/日期不可空白");
                        }
                        else {
                            var p = new Array();
                            p.push({ name: 'FN', value: '藥庫月盤點查詢.xls' });
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                            p.push({ name: 'p1', value: T1Query.getForm().findField('Type').getValue()['Type'] });
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

            { name: 'CHK_REMARK', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        //pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0081/AllM',
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
                    p1: T1Query.getForm().findField('Type').getValue()['Type']
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
                    //T1Cleanup();
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
            var p0 = T1Query.getForm().findField('P0').rawValue == null ? '' : T1Query.getForm().findField('P0').rawValue;
            var p1 = T1Query.getForm().findField('Type').getValue()['Type'];

            var qstring = '?p0=' + p0 + '&p1=' + p1;

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
    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P0').setValue(T1Query.getForm().findField('P0_month').rawValue);
    T1Query.getForm().findField('PP').setValue("PH1S");
});
