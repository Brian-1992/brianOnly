﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    /**
     * Ext Override
     */
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    /**
     * General properties and methods
     */
    var T1GetExcel = '/api/CE0044/Excel';

    var baseUrl = function (actionUrl) {
        return '/api/CE0046/' + actionUrl;
    }

    /**
     *  T1 panel 
     */
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' }, // 院內碼
            { name: 'MMNAME_C', type: 'string' }, // 中文品名
            { name: 'MMNAME_E', type: 'string' }, // 英文品名
            { name: 'BASE_UNIT', type: 'string' }, // 計量單位
            { name: 'INV_QTY', type: 'string' }, // 電腦量(庫存量)
            { name: 'CHK_QTY', type: 'string' }, // 盤點量
            { name: 'MEMO', type: 'string' }, // 備註
            { name: 'OLD_INV_QTY', type: 'string' }, //上期結存
            { name: 'CURRENT_APPLY_AMOUNT', type: 'string' }, // 本月申請總量
            { name: 'ENDL', type: 'string' }
        ]
    });
    // 更新 狀態:  月結日期: 
    var fetchGrid = function () {
        Ext.Ajax.request({
            url: baseUrl('GetChkStatus'),
            params: {
                chk_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                ur_inid: T1Query.getForm().findField('P1').getValue() // 責任中心
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        var item = list[0];
                        T1Query.getForm().findField('CHK_CLOSE_DATE_T').setValue(item.CHK_CLOSE_DATE);
                    }
                    else {
                        T1Query.getForm().findField('CHK_CLOSE_DATE_T').setValue('');
                    }

                    T1Load();
                    T1Tool.moveFirst();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 檢查必填欄位有沒有填值
    var canQuery = function () {
        return (
            (T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null)
        ) === false;
    }
    //查詢月結年月
    var ymComboStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: baseUrl('GetYmCombo'),
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //責任中心代碼
    var urInidComboStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: baseUrl('GetUrInidCombo'),
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'CHK_TIME', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: baseUrl('AllM'),
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    set_ym: T1Query.getForm().findField('P0').getValue(), // 盤點年月 
                    ur_inid: T1Query.getForm().findField('P1').getValue(), // 責任中心 
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, operation, eOpts) {
                // console.log('資料已載入:', records);
            } // end of load
        }
    });

    // 上方查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 70,
            width: 230
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    store: ymComboStore,
                    fieldLabel: '盤點年月',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    allowBlank: false,
                    limit: 10, //限制一次最多顯示10筆
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    width: 180,
                }, {
                    xtype: 'combo',
                    store: urInidComboStore,
                    fieldLabel: '責任中心<br>代碼',
                    name: 'P1',
                    id: 'P1',
                    queryMode: 'local',
                    labelAlign: 'right',
                    anyMatch: true,
                    autoSelect: true,
                    limit: 10, //限制一次最多顯示10筆
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    editable: true,
                    typeAhead: true,
                    forceSelection: true,
                    selectOnFocus: true,
                    width: 180,
                }, {
                    xtype: 'button',
                    text: '查詢',
                    type: 'submit',
                    id: 'T1btn1',
                    handler: function () {
                        msglabel('訊息區:');

                        if (!canQuery()) {
                            Ext.Msg.alert('訊息', '需填盤點年月才能查詢');
                            return;
                        }

                        msglabel('訊息區:');
                        fetchGrid();
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Load();
                        Ext.getCmp('export').setDisabled(true);
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '月結日期',
                    padding: '0 0 0 8',
                    labelAlign: 'right',
                    name: 'CHK_CLOSE_DATE_T',
                    labelWidth: 120
                },
                ]
            }]
        }]
    });

    function T1Load() {
        Ext.getCmp('export').setDisabled(false);

        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    //表單工具列
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'export',
                itemId: 'export',
                text: '匯出',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件 
                            PostForm(baseUrl('Excel'), p);
                        }
                    });
                }
            }
        ]
    });
    // 查詢結果資料列表(上半部)
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
        columns: [
            { xtype: 'rownumberer' },
            { text: "院內碼", dataIndex: "MMCODE", locked: true, width: 85 },
            { text: "中文品名", dataIndex: "MMNAME_C", locked: true, width: 120 },
            { text: "英文品名", dataIndex: "MMNAME_E", locked: true, width: 120 },
            { text: "計量單位", dataIndex: "BASE_UNIT", width: 85 },
            { text: "電腦量", dataIndex: "INV_QTY", width: 85 },
            { text: "盤點量", dataIndex: "CHK_QTY", width: 85 },
            { text: "盤點人員", dataIndex: "CHK_UID", width: 85 },
            { text: "盤點時間", dataIndex: "CHK_TIME", width: 85 },
            { text: "備註", dataIndex: "MEMO", width: 140, },
            { header: "", flex: 1 }
        ],
    });

    // View port view
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
            layout: {
                type: 'border'
            },
            collapsible: false,
            title: '',
            items: [
                {
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    split: true,
                    border: false,
                    items: [T1Grid]
                }
            ]
        }]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});