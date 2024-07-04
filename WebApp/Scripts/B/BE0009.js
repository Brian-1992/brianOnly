Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "發票轉主計";

    var T1Rec = 0;
    var T1LastRec = null;

    var selGrid = 1;

    var st_ActYm = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/GetActymCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, options) {
                if (store.getCount() > 0) {
                    T1Query.getForm().findField('pACT_YM').setValue(store.getAt(0).get('VALUE'));
                    T1Query.getForm().findField('P1').setValue(store.getAt(0).get('EXTRA1'));
                    T1Query.getForm().findField('P2').setValue(store.getAt(0).get('EXTRA2'));
                }
            }
        }
    });

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, options) {
                var MatClassCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                if (MatClassCount > 0) {
                    var temp = [];
                    for (var i = 0; i < MatClassCount; i++) {
                        //if (temp != '') {
                        //    temp += ', ';
                        //}
                        temp.push(store.getAt(i).get('VALUE'));
                    }
                    combo_P0.setValue(temp);
                }
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var AgennoComboGet = '/api/BE0009/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo_1', {
        name: 'P4',
        id: 'P4',
        fieldLabel: '廠商編號',
        allowBlank: true,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: mLabelWidth,
        width: 260,
        padding: '0 1 0 1',
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P4').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP0',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '列帳年月',
                    store: st_ActYm,
                    name: 'pACT_YM',
                    id: 'pACT_YM',
                    labelWidth: 70,
                    width: 250,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    matchFieldWidth: false,
                    forceSelection: true,
                    listConfig: { width: 200 },
                    listeners: {
                        blur: function (field, eOpts) {
                            if (field.getValue() != '' && field.getValue() != null
                                && field.readOnly == false)
                                getINVMARK(field.getValue());
                        },
                        select: function (oldValue, newValue, eOpts) {
                            T1Query.getForm().findField('P1').setValue(newValue.data["EXTRA1"]);
                            T1Query.getForm().findField('P2').setValue(newValue.data["EXTRA2"]);
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '設定完成',
                    id: 'btnFinish',
                    name: 'btnFinish',
                    disabled: true,
                    handler: function () {
                        Ext.MessageBox.confirm('設定完成', '是否確定設定完成?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BE0009/MasterFinish',
                                    method: reqVal_p,
                                    params: {
                                        ACT_YM: T1Query.getForm().findField('pACT_YM').getValue()
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料設定完成成功');
                                            T1Query.down('#btnFinish').setDisabled(true);
                                            T1Load();
                                            T2Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }],
            }, {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    store: st_MatClass,
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 70,
                    width: 250,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '進貨日期',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 70,
                    width: 140,
                    padding: '0 4 0 4',
                    submitValue: true,
                    regexText: '請選擇日期'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '至',
                    labelSeparator: ' ',
                    name: 'P2',
                    id: 'P2',
                    labelWidth: 16,
                    width: 98,
                    padding: '0 2 0 2',
                    submitValue: true,
                    regexText: '請選擇日期'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '發票號碼',
                    name: 'P3',
                    id: 'P3',
                    width: 200
                }, T1FormAgenno, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel('訊息區:');
                        if ((this.up('form').getForm().findField('pACT_YM').getValue() == '' || this.up('form').getForm().findField('pACT_YM').getValue() == null)) {
                            Ext.Msg.alert('提醒', '[列帳年月]不可空白');
                        } else {
                            T1Load();
                            T2Load();
                            T3Grid.getStore().removeAll();
                        }
                    }
                }]
            }]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'UNI_NO', type: 'string' },
            { name: 'DELI_DT', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/Master0',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    act_ym: T1Query.getForm().findField('pACT_YM').getValue(),
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                    viewport.queryById('t1Grid').setTitle('未列帳發票');
                }
                else {
                    viewport.queryById('t1Grid').setTitle('未列帳發票 共' + T1Store.getTotalCount() + '筆');
                }
            }
        }
    });

    function T1Load() {
        T1Store.load();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [{
            text: '加入',
            id: 'btnAdd',
            name: 'btnAdd',
            disabled: true,
            handler: function () {
                var selection = T1Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                } else {
                    let name = '';
                    let invoice = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('INVOICE') + '」<br>';
                        invoice += item.get('INVOICE') + ',';
                    })
                    Ext.MessageBox.confirm('加入', '是否確定加入未列帳發票?<br>' + name, function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/BE0009/MasterAdd',
                                method: reqVal_p,
                                params: {
                                    INVOICE: invoice,
                                    ACT_YM: T1Query.getForm().findField('pACT_YM').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:資料加入成功');

                                        T1Load();
                                        T2Load();
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        }]
    });


    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer',
            width: 35
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 80
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 100
        }, {
            text: "廠商編號",
            dataIndex: 'AGEN_NO',
            width: 80
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 250
        }, {
            text: "統一編號",
            dataIndex: 'UNI_NO',
            width: 90
        }, {
            text: "進貨日期",
            dataIndex: 'DELI_DT',
            width: 200
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                selGrid = 1;

                T3Store.removeAll();

                T1Rec = records.length;
                T1LastRec = records[0];

                if (T1LastRec != null) {
                    T3Load();
                    T4Load();
                    Ext.getCmp('btnAdd').setDisabled(false);
                }
            }
        }
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'UNI_NO', type: 'string' },
            { name: 'DELI_DT', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/Master1',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    act_ym: T1Query.getForm().findField('pACT_YM').getValue(),
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T2Store.removeAll();
                    viewport.queryById('t2Grid').setTitle('列帳發票');
                }
                else {
                    viewport.queryById('t2Grid').setTitle('列帳發票 共' + T2Store.getTotalCount() + '筆');
                }
            }
        }
    });

    function T2Load() {
        T2Store.load();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        buttons: [{
            text: '移除',
            id: 'btnRemove',
            name: 'btnRemove',
            disabled: true,
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                } else {
                    let name = '';
                    let invoice = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('INVOICE') + '」<br>';
                        invoice += item.get('INVOICE') + ',';
                    })
                    Ext.MessageBox.confirm('加入', '是否確定移除列帳發票?<br>' + name, function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/BE0009/MasterRemove',
                                method: reqVal_p,
                                params: {
                                    INVOICE: invoice
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:資料移除成功');

                                        T1Load();
                                        T2Load();
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        }]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer',
            width: 35
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 80
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 100
        }, {
            text: "廠商編號",
            dataIndex: 'AGEN_NO',
            width: 80
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 250
        }, {
            text: "統一編號",
            dataIndex: 'UNI_NO',
            width: 90
        }, {
            text: "進貨日期",
            dataIndex: 'DELI_DT',
            width: 200
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                selGrid = 2;

                T3Store.removeAll();

                T2Rec = records.length;
                T2LastRec = records[0];

                if (T2LastRec != null) {
                    T3Load();
                    T4Load();
                    Ext.getCmp('btnRemove').setDisabled(false);
                }
            }
        }
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INVOICE', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'INVOICE_QTY', type: 'string' },
            { name: 'INVOICE_PRICE', type: 'string' },
            { name: 'INVOICE_AMOUNT', type: 'string' },
            { name: 'REBATESUM', type: 'string' },
            { name: 'DISC_AMOUNT', type: 'string' },
            { name: 'AMTPAID', type:'string'}
        ]
    });

    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/Detail',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var parInvoice = '';
                if (selGrid == 1)
                    parInvoice = T1LastRec.data.INVOICE;
                else if (selGrid == 2)
                    parInvoice = T2LastRec.data.INVOICE;

                // 載入前將查詢條件P0值代入參數
                var np = {
                    invoice: parInvoice
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T3Load() {
        T3Store.load();
    }
    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 300
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 300
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',
            width: 100
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        }, {
            text: "發票數量",
            dataIndex: 'INVOICE_QTY',
            width: 80
        }, {
            text: "發票單價",
            dataIndex: 'INVOICE_PRICE',
            width: 100
        }, {
            text: "小計總價",
            dataIndex: 'INVOICE_AMOUNT',
            width: 100
        }, {
            text: "折讓金額",
            dataIndex: 'REBATESUM',
            width: 100
        }, {
            text: "優惠金額",
            dataIndex: 'DISC_AMOUNT',
            width: 100
        }, {
            text: "實付金額",
            dataIndex: 'AMTPAID',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T3Rec = records.length;
                T3LastRec = records[0];
            }
        }
    });

    function getINVMARK(parAct_ym) {
        Ext.Ajax.request({
            url: '/api/BE0009/getINVMARK',
            method: reqVal_p,
            params: { act_ym: parAct_ym },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        if (tb_data[0].INVMARK == '1') {
                            T1Query.down('#btnFinish').setDisabled(true);
                        } else {
                            T1Query.down('#btnFinish').setDisabled(false);
                        }
                    } else {
                        T1Query.down('#btnFinish').setDisabled(false);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' }
        ]
    });

    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0009/DetailForm',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var parInvoice = '';
                if (selGrid == 1)
                    parInvoice = T1LastRec.data.INVOICE;
                else if (selGrid == 2)
                    parInvoice = T2LastRec.data.INVOICE;

                // 載入前將查詢條件P0值代入參數
                var np = {
                    invoice: parInvoice
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T4Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T4Form.loadRecord(records[0]);
                    }
                    else {
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T4Form.getForm().reset();
                    }
                }
            }
        }
    });

    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }

    var mLabelWidth = 160;
    var mWidth = 360;
    var T4Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        layout: {
            type: 'table',
            columns: 3,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: 160,
            width: 360,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [
            { fieldLabel: '發票小計金額', name: 'F1' },
            { fieldLabel: '優惠及折讓金額', name: 'F2' },
            { fieldLabel: '總實付金額', name: 'F3' }
        ]
    });
    
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't1top',
            region: 'north',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            height: '10%',
            items: [T1Query]
        }, {
            itemId: 't1center',
            region: 'center',
            layout: 'border',
            collapsible: false,
            title: '',
            border: false,
            height: '90%',
            items: [{
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '未列帳發票',
                border: false,
                width: '50%',
                minWidth: 50,
                minHeight: 140,
                split: true,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                region: 'east',
                layout: 'fit',
                title: '列帳發票',
                split: true,
                width: '50%',
                minWidth: 50,
                minHeight: 140,
                border: false,
                items: [T2Grid]
            }, {
                itemId: 't3Grid',
                region: 'south',
                collapsible: false,
                title: '發票明細',
                height: '50%',
                split: true,
                items: [T3Grid]
            }, {
                itemId: 't4Form',
                region: 'south',
                collapsible: false,
                title: '',
                height: '5%',
                //  split: true,
                items: [T4Form]
            }]
        }]
    });
});