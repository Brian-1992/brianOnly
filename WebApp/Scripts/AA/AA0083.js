﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1Get = '/api/AA0083/All';
    var T2Get = '/api/AA0083/GetUseQTY';
    var T3Get = '/api/AA0083/GetDetail';
    var T1Name = '各庫庫存情形';
    var T2Name = '近6個月內消耗表';

    var T1Rec = 0;
    var T1LastRec = null;
    var IsPageLoad = true;

    function T1Load() {
        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();

        T1Store.loadPage(1);
        T2Store.loadPage(1);
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Menu').unmask();
        var f = T1Form.getForm();
        f.reset();

        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            if (f.xtype === "combo" || f.xtype === "datefield") {
                f.readOnly = true;
            }
            else {
                f.setReadOnly(true);
            }
        });

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }


    var isNew = false;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'WH_NO',
            'WH_NAME',
            'P_INV_QTY',
            'INQTY',
            'OUTQTY',
            'INV_QTY',
            'ONWAY_QTY',
            'ADJ_QTY',
            'OTHER_INQTY',
            'OTHER_OUTQTY',
            'SAFE_QTY',
            'OPER_QTY',
            'UPRICE',
            'USE_QTY'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T3Store.load();
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        id: 'P0',
        fieldLabel: '藥品',
        labelAlign: 'right',
        labelWidth: 60,
        width: 230,
        allowBlank: false,
        fieldCls: 'required',
        padding: '0 4 0 4',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0083/GetMMCodeDocd', //指定查詢的Controller路徑
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: '',
                m_stroeid: ''
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Query.getForm().findField('P1').setValue(r.get('MMNAME_E'));
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        defaults: {
            labelAlign: "right",
            labelWidth: 40
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [T1QueryMMCode,
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            if (Ext.getCmp('P0').validate()) {
                                T1Load();
                            }
                            else {
                                Ext.Msg.alert('訊息', '[藥品]為必填條件');
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus();
                        }
                    }]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'displayfield',
                    id: 'P1',
                    name: 'P1',
                    labelAlign: 'right',
                    padding: '4 4 0 4',
                    labelWidth: 60,
                    width: '100%',
                    fieldLabel: '品名'
                }]
            }
            ]
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 2
        },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'displayfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right"
        },
        items: [{
            fieldLabel: '現品總量',
            width: 250,
            name: 'INV_QTY'
        }, {
            fieldLabel: '消耗總量(本月)',
            width: 250,
            name: 'USE_QTY'
        }, {
            fieldLabel: '第一次進貨日期',
            width: 250,
            name: 'MIN_ACCOUNTDATE',
        }, {
            fieldLabel: '最後進貨日期',
            width: 250,
            name: 'MAX_ACCOUNTDATE',
        }, {
            fieldLabel: '廠商碼',
            width: 250,
            name: 'AGEN_NAME',
        }, {
            fieldLabel: '合約碼',
            width: 250,
            name: 'CONTRACT_TYPE',
        }, {
            fieldLabel: '合約單價',
            width: 250,
            name: 'M_CONTPRICE',
        }, {
            fieldLabel: '優惠合約單價',
            width: 250,
            name: 'DISC_CPRICE',
        }, {
            fieldLabel: '最小撥補量',
            width: 250,
            name: 'MIN_ORDQTY',
        }, {
            fieldLabel: '儲位',
            width: 250,
            name: 'STORE_LOC',
        }
        //, {
        //    fieldLabel: '單價',
        //    width: 250,
        //    name: 'UPRICE',
        //}
        ]

    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        uses: [
            'Ext.ux.exporter.Exporter'
        ],
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],

        columns: [{
            xtype: 'rownumberer'
        },{
            text: "庫別",
            dataIndex: 'WH_NAME',
            width: 100,
        }, {
            text: "上月結存",
            dataIndex: 'P_INV_QTY',
            width: 80
        }, {
            text: "本月進",
            dataIndex: 'INQTY',
            width: 80
        }, {
            text: "本月出",
            dataIndex: 'OUTQTY',
            width: 80
        }, {
            text: "現品量",
            dataIndex: 'INV_QTY',
            width: 80
        }, {
            text: "在途量",
            dataIndex: 'ONWAY_QTY',
            width: 80
        }, {
            text: "調整量",
            dataIndex: 'ADJ_QTY',
            width: 80
        }, {
            text: "耗用量",
            dataIndex: 'USE_QTY',
            width: 80
        }, {
            text: "其他進",
            dataIndex: 'OTHER_INQTY',
            width: 80
        }, {
            text: "其他出",
            dataIndex: 'OTHER_OUTQTY',
            width: 80
        },{
            text: "安全量",
            dataIndex: 'SAFE_QTY',
            width: 80
        }, {
            text: "作業量",
            dataIndex: 'OPER_QTY',
            width: 80
        }, {
            text: "",
            flex: 1,
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                if (records.length) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    //T3Store.load();
                    setFormT1a();
                }
            }
        }
    });
    function setFormT1a() {
        var param0 = '';
        if (T1LastRec) {
            var f = T1Form.getForm();
            viewport.down('#form').expand();
        }
        else {
            T1Form.getForm().reset();
        }
    }

    //menu
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DATA_YM', type: 'string' },
            { name: 'USE_QTY', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T2Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        uses: [
            'Ext.ux.exporter.Exporter'
        ],
        columns: [{
            xtype: 'rownumberer'
        },{
            text: "月份",
            dataIndex: 'DATA_YM',
            width: 100,
        }, {
            text: "消耗量",
            dataIndex: 'USE_QTY',
            flex: 1,
            sortable: false
        }]
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INV_QTY', type: 'string' },
            { name: 'USE_QTY', type: 'string' },
            { name: 'MIN_ACCOUNTDATE', type: 'string' },
            { name: 'MAX_ACCOUNTDATE', type: 'string' },
            { name: 'AGEN_NAME', type: 'string' },
            { name: 'CONTRACT_TYPE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' }
        ]
    });

    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 20,
        remoteSort: true,
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    // WH_NAME: T1LastRec.get('WH_NAME'),
                    WH_NAME: '', // 目前WH_NAME參數沒用到
                    MMCODE: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                //if (T2Store.getCount()) T2Store.getRootNode().removeAll();
                if (successful) {
                    var f = T1Form.getForm();
                    f.findField('INV_QTY').setValue(records[0].get('INV_QTY'));
                    f.findField('USE_QTY').setValue(records[0].get('USE_QTY'));
                    f.findField('MIN_ACCOUNTDATE').setValue(records[0].get('MIN_ACCOUNTDATE'));
                    f.findField('MAX_ACCOUNTDATE').setValue(records[0].get('MAX_ACCOUNTDATE'));
                    f.findField('AGEN_NAME').setValue(records[0].get('AGEN_NAME'));
                    f.findField('CONTRACT_TYPE').setValue(records[0].get('CONTRACT_TYPE'));
                    f.findField('M_CONTPRICE').setValue(records[0].get('M_CONTPRICE'));
                    f.findField('DISC_CPRICE').setValue(records[0].get('DISC_CPRICE'));
                    f.findField('STORE_LOC').setValue(records[0].get('STORE_LOC'));
                    f.findField('MIN_ORDQTY').setValue(records[0].get('MIN_ORDQTY'));
                }
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T3Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
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
            split: true,
            width: '60%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 't2Menu',
            region: 'east',
            layout: 'fit',
            title: T2Name,
            split: true,
            width: '30%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T2Grid]
        },
        {
            itemId: 'form',
            region: 'south',
            collapsible: true,
            floatable: true,
            split: true,
            width: '20%',
            minWidth: 120,
            minHeight: 140,
            //title: '修改',
            collapsed: false,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
    msglabel('');
    IsPageLoad = false;
});