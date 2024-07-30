Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {


    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var mLabelWidth = 70;
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
                    xtype: 'monthfield',
                    fieldLabel: '資料年月',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 60,
                    width: 130,
                    padding: '0 4 0 4'
                    //,value: new Date()
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelSeparator: '',
                    labelWidth: 10,
                    width: 80,
                    padding: '0 4 0 4'
                    //,value: new Date()
                },{
                    xtype: 'textfield',
                    fieldLabel: '電腦編號',
                    name: 'P2',
                    id: 'P2',
                    //enforceMaxLength: true, // 限制可輸入最大長度
                    //maxLength: 6, // 可輸入最大長度為100
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '藥品名稱',
                    name: 'P3',
                    id: 'P3',
                    //enforceMaxLength: true,
                    //maxLength: 6,
                    padding: '0 4 0 4'
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
                    }
                }

            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DATA_YM', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MUSE_QTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },

            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_TIME_T', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DATA_YM', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/GA0006/All',
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
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    //var T1Store = Ext.create('WEBAPP.store.TC_PURUNCOV', { // 定義於/Scripts/app/store/PhVender.js
    //    listeners: {
    //        beforeload: function (store, options) {
    //            // 載入前將查詢條件P0~P4的值代入參數
    //            var np = {
    //                p0: T1Query.getForm().findField('P0').rawValue,
    //                p1: T1Query.getForm().findField('P1').rawValue,
    //                p2: T1Query.getForm().findField('P2').getValue(),
    //                p3: T1Query.getForm().findField('P3').getValue()
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});
    function T1Load() {
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T1Tool.moveFirst();
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
        }, {
            text: "資料年月",
            dataIndex: 'DATA_YM',
            width: 70
        }, {
            text: "電腦編號",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "月消耗量",
            dataIndex: 'MUSE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 70
        }, {
            text: "建立時間",
            dataIndex: 'CREATE_TIME_T',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                
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
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});
