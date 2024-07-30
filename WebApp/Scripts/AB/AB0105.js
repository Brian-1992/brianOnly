Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '/api/AB0102/Excel';
    var mLabelWidth = 80;
    var mWidth = 200;
    var radioSelected = 'isCombo';
    var isRadioChanged = false;

    var towhStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var frwhStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 180,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0105/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
                wh_no: T1Query.getForm().findField('P0').getValue()
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("P3").setValue(r.get('MMCODE'));
                }
            }
        }
    });
    var flowidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function getTowhCombo() {
        Ext.Ajax.request({
            url: '/api/AB0105/GetTowhCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var towhs = data.etts;
                    if (towhs.length > 0) {
                        towhStore.add({
                            VALUE: '',
                            TEXT: ''
                        });
                        for (var i = 0; i < towhs.length; i++) {
                            towhStore.add({
                                VALUE: towhs[i].VALUE,
                                TEXT: towhs[i].TEXT
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTowhCombo();
    

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'DATA_DATE',
            'DATA_BTIME',
            'DATA_ETIME',
            'WH_NO',
            'MMCODE',
            'VISIT_KIND',
            'CONSUME_QTY',
            'STOCK_UNIT',
            'INSU_QTY',
            'HOSP_QTY',
            'PARENT_ORDERCODE',
            'PARENT_CONSUME_QTY',
            'CREATEDATETIME',
            'PROC_ID',
            'PROC_MSG',
            'PROC_TYPE'
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AB.AB0105', {
        pageSize: 50, // 每頁顯示筆數
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    data_ym_s: T1Query.getForm().findField('P0').rawValue,
                    data_ym_e: T1Query.getForm().findField('P1').rawValue,
                    towh: T1Query.getForm().findField('P2').getValue(),
                    mmcode: T1Query.getForm().findField('P3').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

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
                    xtype: 'monthfield',
                    fieldLabel: '月結年月',
                    name: 'P0',
                    id: 'P0',
                    width: 150,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    fieldCls: 'required',
                    value: new Date((new Date()).getFullYear(), (new Date()).getMonth() - 1,)
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    // enforceMaxLength: true,
                    //maxLength: 5,
                    //minLength: 5,
                    //regexText: '請填入民國年月',
                    //regex: /\d{5,5}/,
                    fieldCls: 'required',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 78,
                    padding: '0 4 0 4',
                    //format: 'Xm',
                    value: new Date()
                },
                {
                    xtype: 'combo',
                    store: towhStore,
                    name: 'P2',
                    id: 'P2',
                    fieldLabel: '入庫庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                mmCodeCombo,
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        if (!T1Query.getForm().findField('P0').getValue() ||
                            !T1Query.getForm().findField('P1').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">月結年月</span>為必填');
                            return;
                        }
                        if (T1Query.getForm().findField('P0').rawValue < '10903' || 
                            T1Query.getForm().findField('P1').rawValue < '10903') {
                            Ext.Msg.alert('提醒', '<span style="color:red">月結年月</span>需大於10903');
                            return;
                        }
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        f.reset();

                        msglabel('訊息區:');
                    }
                }
            ]
        },
        ]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
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
            },
            {
                text: "入庫庫房",
                dataIndex: 'TOWH',
            width: 130
        }, {
                text: "月結年月",
            dataIndex: 'SET_YM',
            width: 70
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
        }, {
                text: "核撥數量",
            dataIndex: 'APVQTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
        }, {
                text: "核撥時間",
                dataIndex: 'APVTIME',
            width: 120
        }, {
            text: "點收數量",
            dataIndex: 'ACKQTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "點收時間",
            dataIndex: 'ACKTIME',
            width: 120
        }, {
            text: "強迫點收數量",
            dataIndex: 'UPDATE_IP',
            width: 110,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "過帳時間",
            dataIndex: 'POST_TIME',
            width: 120
        }, {
            text: "單據號碼",
            dataIndex: 'DOCNO',
            width: 100
        }, {
            text: "單據類別",
            dataIndex: 'DOCTYPE',
            width: 100
        }, {
            text: "申請部門",
            dataIndex: 'APPDEPT',
            width: 100
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME',
            width: 120
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH',
            width: 130
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS',
            width: 100
        }]
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    //#region viewport
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
        }
        ]
    });
    //#endregion

});