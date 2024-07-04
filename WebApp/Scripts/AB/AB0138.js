
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Name = "公藥補撥單管理";
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var reportUrl = '/Report/A/AB0138.aspx';
    var getDocAppAmout = '/api/AB0138/GetDocAppAmout';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function setT1QueryData() {
        Ext.Ajax.request({
            url: '/api/AB0138/GetLoginInfo',
            method: reqVal_p, // by default GET
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var storeItems = data.etts;
                    if (storeItems.length > 0) {
                        T1Query.getForm().findField('P0').setValue(storeItems[0].INID);
                        T1Query.getForm().findField('P1').setValue(storeItems[0].INIDNAME);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setT1QueryData();

    var st_isdef = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0138/GetIsDefCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 100,
            labelAlign: 'right'
        },
        border: false,
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '補撥單位',
                name: 'P0'
            }, {
                xtype: 'displayfield',
                fieldLabel: '',
                name: 'P1'
            }
            /*, {
                xtype: 'button',
                text: '查詢',
                handler: T1Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                    msglabel('');
                }
            }*/
        ]
    });
    
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'STATUS', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0138/AllM',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    msglabel('訊息區:');
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AB0138/CreateM',
                        method: reqVal_p,
                        success: function (response) {
                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T2Store.removeAll();
                                T1Grid.getSelectionModel().deselectAll();
                                T1Load();
                                msglabel('訊息區:新增成功');
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                itemId: 'delete', text: '取消', disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('取消', '是否確定取消補撥單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0138/DeleteM',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '取消補撥單號<br>' + name + '成功');
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                }
            },
            {
                itemId: 'apply', text: '送出', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('送出', '是否確定送出？補撥單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0138/Apply',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:送出成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                }
            }, {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
                    showReport(T1LastRec.data["DOCNO"]);
                }
            }
        ]
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
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "補撥單編\號",
            dataIndex: 'DOCNO',
            width: 150
        }, {
            text: "補撥時間",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    /*if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }*/
                }
            },
            selectionchange: function (model, records) {
                //viewport.down('#form').expand();
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        T2Cleanup();
        T2Query.getForm().reset();
        T2Load();
    }
    

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;
    
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '合計',
                padding: '0 0 0 8',
                labelAlign: 'right',
                name: 'APP_AMOUT',
                labelWidth: 70,
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        return '<span style="color:red">' + val + '</span>';
                    }
                }
            }
        ]
    });


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'APPAMT', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'HIGH_QTY', type: 'string' },
            { name: 'ISDEF', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0138/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1LastRec.data.DOCNO
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    if (records.length > 0) {
                        T1Tool.down('#print').setDisabled(false);
                        T1Tool.down('#apply').setDisabled(false);
                    }
                    reCalAppAmout();
                }
            }
        }
    });
    function T2Load() {
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }
    var T2FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '藥材代碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0138/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'DISC_UPRICE', 'INV_QTY', 'HIGH_QTY'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {

            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('DISC_UPRICE').setValue(r.get('DISC_UPRICE'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));

                if (T2Form.getForm().findField('APPQTY').getValue()) {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APPAMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                }
            }
        }
    });
    var T2Form = Ext.widget({
        //hidden: true,
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        autoScroll: true,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [
            {
                name: 'x',
                xtype: 'hidden'
            }, {
                name: 'DOCNO',
                xtype: 'hidden'
            },
                T2FormMMCode,
            {
                xtype: 'displayfield',
                fieldLabel: '藥材名稱',
                name: 'MMNAME_C'
            },
            {
                fieldLabel: '補撥數量',
                name: 'APPQTY',
                allowBlank: false,
                enforceMaxLength: true,
                maxLength: 10,
                maskRe: /[0-9]/,
                fieldCls: 'required',
                readOnly: true,
                listeners: {
                    blur: function () {
                        
                        // 小計 = 單價 * 補撥數量
                        var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                        var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                        T2Form.getForm().findField('APPAMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                    }
                }
            }, 
            {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'BASE_UNIT'
            }, {
                xtype: 'displayfield',
                fieldLabel: '單價',
                name: 'DISC_UPRICE'
            }, {
                xtype: 'displayfield',
                fieldLabel: '小計',
                name: 'APPAMT'
            }, {
                xtype: 'displayfield',
                fieldLabel: '補撥前結存量',
                name: 'INV_QTY'
            }, {
                xtype: 'displayfield',
                fieldLabel: '基準量',
                name: 'HIGH_QTY'
            }, {
                xtype: 'combo',
                fieldLabel: '定時補撥',
                name: 'ISDEF',
                store: st_isdef,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required'
            }
        ],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                var isSub = true;
                if (this.up('form').getForm().findField('APPQTY').getValue() == '0') {
                    Ext.Msg.alert('提醒', '補撥數量不可為0');
                    isSub = false;
                }
                else {
                    var highqty = 9999;
                    if (this.up('form').getForm().findField('HIGH_QTY').getValue() != null) {
                        highqty = Number(this.up('form').getForm().findField('HIGH_QTY').getValue());
                    }
                    var appqty = 0;
                    if (this.up('form').getForm().findField('APPQTY').getValue() != null) {
                        appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
                    }
                    if (appqty > highqty) {
                        Ext.Msg.alert('提醒', '補撥數量不可超過基準量!');
                        isSub = false;
                    }
                }
                if (isSub) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    });
                }
            }
        }, {
                itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                    T2Cleanup();
                    T2Load();
                    viewport.down('#form').collapse();
                }
        }]
    });

    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T2Cleanup();
                    T2Load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        f.findField('MMCODE').setReadOnly(true);
        f.findField('APPQTY').setReadOnly(true);
        f.findField('ISDEF').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    T2Set = '../../../api/AB0138/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/AB0138/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let docno = '';
                        let mmcode = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('MMCODE') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            mmcode += item.get('MMCODE') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0138/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        MMCODE: mmcode
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        viewport.down('#form').expand();
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('T2Model');
            //var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1LastRec.data.DOCNO);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
            f2.findField('APPQTY').setValue('1');
            f2.findField('ISDEF').setValue('N');
            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }
        
        f2.findField('x').setValue(x);
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('ISDEF').setReadOnly(false);
        
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "藥材代碼",
                dataIndex: 'MMCODE',
                width: 100,
                sortable: true
            }, {
                text: "藥材名稱",
                dataIndex: 'MMNAME_C',
                width: 200,
                sortable: true
            }, {
                text: "補撥數量",
                dataIndex: 'APPQTY',
                style: 'text-align:left',
                width: 100, align: 'right'
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 50,
                sortable: true
            }, {
                text: "單價",
                dataIndex: 'DISC_UPRICE',
                style: 'text-align:left',
                width: 80, align: 'right'
            }, {
                text: "小計",
                dataIndex: 'APPAMT',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                text: "補撥前結存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                width: 120, align: 'right'
            }, {
                text: "基準量",
                dataIndex: 'HIGH_QTY',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                text: "定時補撥",
                dataIndex: 'ISDEF',
                style: 'text-align:left',
                width: 100
            }, {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });
    function setFormT2a() {
        T2Grid.down('#edit').setDisabled(T2Rec === 0);
        T2Grid.down('#delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
        }
        else {
            T2Form.getForm().reset();
        }
    }

    function showReport(P0) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?p0=' + P0
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
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T2Form]
        }
        ]
    });

    // 重算當前補撥單的補撥總金額
    reCalAppAmout = function () {
        if (T1LastRec) {
            Ext.Ajax.request({
                url: getDocAppAmout,
                params: {
                    docno: T1LastRec.data.DOCNO
                },
                method: reqVal_p,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        T2Query.getForm().findField('APP_AMOUT').setValue(data.msg);
                    }
                },
                failure: function (response, options) {

                }
            });
        }
    }

    function accMul(arg1, arg2) {
        var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
        try {
            m += s1.split(".")[1].length;
        } catch (e) { }
        try {
            m += s2.split(".")[1].length;
        } catch (e) { }
        return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m);
    }

    viewport.down('#form').collapse();
    T1Load(); 
    T1Query.getForm().findField('P0').focus();
});
