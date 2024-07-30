﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name2 = "日平均及前六個月消耗查詢";
    
    var T1LastRec = null;
    
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AB0129/GetWH_NOComboOne';  //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)
    var Mm_codeComboGet = '../../../api/AB0129/GetMM_CODEComboOne';  //院內碼
    var NowMonthGet = '../../../api/AB0129/GetNowMonth';  //現在月份
    var LastUpdateGet = '../../../api/AB0129/GetLastUpdateDate';  //最後更新日期

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
    function setWh_noComboData() {
        Ext.Ajax.request({
            url: WH_NOComboGet,
            params: { limit: 10, page: 1, start: 0 },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P0 = T1Query.getForm().findField('P0');
                    if (tb_wh_no.length == 1) {
                        //1筆資料時將
                        wh_noStore.add({ WH_NO: tb_wh_no[0].WH_NO, WH_NAME: tb_wh_no[0].WH_NAME });
                        combo_P0.setValue(tb_wh_no[0].WH_NO);
                    }
                    else {
                        combo_P0.setDisabled(false);
                        if (tb_wh_no.length > 0) {
                            for (var i = 0; i < tb_wh_no.length; i++) {
                                wh_noStore.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setWh_noComboData();

    //現在月份
    function setNowMonth() {
        Ext.Ajax.request({
            url: NowMonthGet,
            params: { limit: 10, page: 1, start: 0 },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('now_month').setValue(data.etts[0]);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setNowMonth();

    //最後更新日期
    function setLastUpdateDate() {
        Ext.Ajax.request({
            url: LastUpdateGet,
            params: { limit: 10, page: 1, start: 0 },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('last_update_date').setValue(data.etts[0]);
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setLastUpdateDate();

    //院內碼
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        fieldCls: 'required',
        width: 300,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0129/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //P0是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T1Query.getForm().findField('P2').setValue(r.get('MMNAME_C'));
                T1Query.getForm().findField('P3').setValue(r.get('MAT_CLSNAME'));
                T1Query.getForm().findField('P4').setValue(r.get('BASE_UNIT'));
                T1Query.getForm().findField('P5').setValue(r.get('UNITRATE'));
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
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
            bodyStyle: 'padding: 3px 5px;',
            items: [{
                xtype: 'combo',
                store: wh_noStore,
                fieldLabel: '庫房代碼',
                name: 'P0',
                id: 'P0',
                queryMode: 'local',
                fieldCls: 'required',
                allowBlank: false,
                anyMatch: true,
                autoSelect: true,
                displayField: 'WH_NO',
                valueField: 'WH_NO',
                requiredFields: ['WH_NAME'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                        T1Query.getForm().findField('P1').setValue("");
                        T1Query.getForm().findField('P2').setValue("");
                        T1Query.getForm().findField('P3').setValue("");
                        T1Query.getForm().findField('P4').setValue("");
                        T1Query.getForm().findField('P5').setValue("");
                    }
                },
                padding: '0 4 0 4'
            },
                T1QuryMMCode,
            {
                xtype: 'displayfield',
                name: 'now_month',
                id: 'now_month',
                fieldLabel: '月份',
                maxLength: 200 // 可輸入最大長度為200
            },
            {
                xtype: 'displayfield',
                name: 'last_update_date',
                id: 'last_update_date',
                labelWidth: 100,
                fieldLabel: '最後更新日期',
                maxLength: 200 // 可輸入最大長度為200
            }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [{
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'P2',
                id: 'P2',
                maxLength: 200 // 可輸入最大長度為200
            },
            {
                xtype: 'displayfield',
                fieldLabel: '品項別',
                name: 'P3',
                id: 'P3',
                maxLength: 200 // 可輸入最大長度為200
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'P4',
                id: 'P4',
                maxLength: 200 // 可輸入最大長度為200
            },
            {
                xtype: 'displayfield',
                fieldLabel: '出貨單位',
                name: 'P5',
                id: 'P5',
                maxLength: 200 // 可輸入最大長度為200
            },
            {
                xtype: 'button',
                text: '查詢',
                style: 'margin:0px 5px;',
                handler: T1Load
            },
            {
                xtype: 'button',
                text: '清除',
                style: 'margin:0px 5px;',
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

    var T1Store = Ext.create('WEBAPP.store.AB0129', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
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
                        Ext.Msg.alert('提醒', '查無資料!');
                        T1Form.getForm().reset();
                    }
                }
                T1Form.loadRecord(T1Store.data.items[0]);
            }
        }
    });

    function T1Load() {
        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)) {

            Ext.Msg.alert('訊息', '需填庫房代碼及院內碼才能查詢');
        } else {
            T1Store.load({
                params: {
                    start: 0
                }
            });
        }
        msglabel('訊息區:');
    }

    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        width: 1500,
        layout: 'hbox',
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 20',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }],
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'vbox',
                width: 350,
                bodyStyle: 'padding: 3px 5px;',
                defaults: {
                    xtype: 'textfield',
                    labelAlign: 'left',
                    readOnly: true,
                    labelWidth: 50,
                    width: mWidth,
                    padding: '4 0 4 20',
                    msgTarget: 'side',
                    readOnly: true
                },
                items: [
                    {
                        xtype: 'displayfield', value: '日平均消耗',
                        fieldStyle: 'font-weight: bold;color: #0043ff;font-size: 20px;', padding: '20 0 4 20'
                    },
                    { fieldLabel: '10天', name: 'USE_QTY_10' },
                    { fieldLabel: '14天', name: 'USE_QTY_14' },
                    { fieldLabel: '90天', name: 'USE_QTY_90' }
                ]
            },
            {
                xtype: 'panel',
                border: false,
                layout: 'vbox',
                width: 300,
                bodyStyle: 'padding: 3px 5px;',
                defaults: {
                    xtype: 'textfield',
                    labelAlign: 'left',
                    readOnly: true,
                    labelWidth: 80,
                    width: mWidth,
                    padding: '4 0 4 20',
                    msgTarget: 'side',
                    readOnly: true
                },
                items: [
                    {
                        xtype: 'displayfield', value: '前六個月消耗',
                        fieldStyle: 'font-weight: bold;color: #0043ff;font-size: 20px;', padding: '20 0 4 20'
                    },
                    { fieldLabel: '前第一個月', name: 'USE_QTY_1M' },
                    { fieldLabel: '前第二個月', name: 'USE_QTY_2M' },
                    { fieldLabel: '前第三個月', name: 'USE_QTY_3M' },
                    { fieldLabel: '前第四個月', name: 'USE_QTY_4M' },
                    { fieldLabel: '前第五個月', name: 'USE_QTY_5M' },
                    { fieldLabel: '前第六個月', name: 'USE_QTY_6M' },
                    { fieldLabel: '三個月平均', name: 'USE_QTY_3MA' },
                    { fieldLabel: '六個月平均', name: 'USE_QTY_6MA' }
                ]
            }
        ]
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
            itemId: 't1Form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});