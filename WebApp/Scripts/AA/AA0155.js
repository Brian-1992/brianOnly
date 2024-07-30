Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '戰備調整維護';
    var userId, userName, userInid, userInidName, userTaskId;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'FRWH_NAME', type: 'string' },
            { name: 'TOWH_NAME', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'WH_KIND', type: 'string' }
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
            url: '/api/AA0155/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                    d0: T1Query.getForm().findField('D0').rawValue,
                    d1: T1Query.getForm().findField('D1').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        FrwhLoadF('');
        T1Tool.moveFirst();
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'UP', type: 'string' },
            { name: 'AMT', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Tool.moveFirst();
        }
    }

    var MclassStoreQ = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetMclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    IS_QUERY: 'Y'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        autoLoad: true
    });
    var MclassStoreF = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetMclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    IS_QUERY: 'N'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        autoLoad: true
    });

    var FrwhStoreQ = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NO_NAME'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var FrwhStoreF = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NO_NAME', 'TOWH', 'APPLY_KIND'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: false
    });
    function FrwhLoadQ(wh_kind) {
        FrwhStoreQ.getProxy().setExtraParam("WH_KIND", wh_kind);
        FrwhStoreQ.load();
    }
    function FrwhLoadF(wh_kind) {
        FrwhStoreF.getProxy().setExtraParam("WH_KIND", wh_kind);
        FrwhStoreF.load();
    }

    var FlowidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    function FlowidLoad() {
        FlowidStore.load({
            params: {
                start: 0
            }
        });
    }

    var UserInfoStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0155/GetStatCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    function UserInfoLoad() {
        UserInfoStore.load(function (records, operation, success) {
            if (success) {
                var r = records[0];
                userId = r.get('TUSER');
                userName = r.get('UNA');
                userInid = r.get('INID');
                userInidName = r.get('INID_NAME');
            }
        });
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '申請單',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4'
            },
            {
                xtype: 'datefield',
                fieldLabel: '異動日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P3',
                id: 'P3',
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4',
                store: MclassStoreQ,
                displayField: 'TEXT',
                valueField: 'VALUE',
                //editable: false,
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all', 
                listeners: {
                    select: function (c, r, i, e) {
                        FrwhLoadQ(r.get('EXTRA1'));
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '出庫庫房',
                name: 'P4',
                id: 'P4',
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4',
                store: FrwhStoreQ,
                displayField: 'WH_NO_NAME',
                valueField: 'WH_NO',
                //editable: false,
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
            },
            {
                xtype: 'combo',
                fieldLabel: '狀態',
                name: 'P6',
                id: 'P6',
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4',
                store: FlowidStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                //editable: false,
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    Ext.getCmp('eastform').collapse();
                    msglabel('訊息區:');

                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');
                }
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AA0155/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AA0155/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '撤銷',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('撤銷', '是否確定撤銷申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0155/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:撤銷成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                            msglabel('訊息區:' + data.msg);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                text: '過帳',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否過帳?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                myMask.show();
                                Ext.Ajax.request({
                                    url: '/api/AA0155/UpdateStatusBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        myMask.hide();
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        myMask.hide();
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            }
        ]
    });

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        Tabs.setActiveTab('Form');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        FrwhStoreF.removeAll();

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("TOWH");
            // u.setReadOnly(false);
            u.clearInvalid();
            f.findField('FRWH').setReadOnly(false);
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField("DOCNO").setValue('系統自編');
            f.findField("FLOWID").setValue('退貨中');

            var year = new Date().getFullYear();
            var month = (new Date().getMonth() + 1) > 9 ? (new Date().getMonth() + 1).toString() : "0" + (new Date().getMonth() + 1).toString();
            var date = (new Date().getDate()) > 9 ? new Date().getDate().toString() : "0" + new Date().getDate().toString();
            var chtToday = (year - 1911).toString() + month.toString() + date.toString();
            // f.findField("APPTIME").setValue(getChtToday());
            f.findField("APPTIME").setValue(chtToday);
        }
        else {
            FrwhLoadF(f.findField('WH_KIND').getValue());
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('FRWH').setReadOnly(false);
        }
        f.findField('x').setValue(x);
        
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                fieldLabel: 'WH_KIND',
                name: 'WH_KIND',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單據號碼',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '狀態',
                name: 'FLOWID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '調整日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'MAT_CLASS',
                id: 'MAT_CLASS',
                store: MclassStoreF,
                displayField: 'TEXT',
                valueField: 'VALUE',
                readOnly: true,
                //editable: false,
                allowBlank: false,
                fieldCls: 'required',
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                listeners: {
                    select: function (combo, record, eOpts) {
                        FrwhLoadF(record.get("EXTRA1"));
                        var f = T1Form.getForm();
                        f.findField('FRWH').setValue('');
                        f.findField('TOWH').setValue('');
                        f.findField('APPLY_KIND').setValue('');
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '出庫庫房',
                name: 'FRWH',
                id: 'FRWH',
                store: FrwhStoreF,
                displayField: 'WH_NO_NAME',
                valueField: 'WH_NO',
                readOnly: true,
                //editable: false,
                allowBlank: false,
                fieldCls: 'required',
                anyMatch: true,
                typeAhead: true,
                // forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                listeners: {
                    select: function (combo, record, eOpts) {
                        var f = T1Form.getForm();
                        f.findField('TOWH').setValue(record.get("TOWH"));
                        f.findField('APPLY_KIND').setValue(record.get("APPLY_KIND"));
                    }
                }
            },
            {
                xtype: 'textfield',
                fieldLabel: '入庫庫房',
                name: 'TOWH',
                id: 'TOWH',
                readOnly: true,
                //editable: false,
                allowBlank: false,
                fieldCls: 'required'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '調整類別',
                name: 'APPLY_KIND',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                                //T1Set = '';
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T2Store.removeAll();
                            T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                        case "R":
                            T1Load();
                            msglabel('訊息區:資料退回成功');
                            T1Set = '';
                            break;
                    }

                    T1Cleanup();
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
                            break;
                    }
                }
            });
        }
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
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
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 120
            },
            {
                text: "調整日期",
                dataIndex: 'APPTIME',
                width: 90
            },
            {
                text: "調整人員",
                dataIndex: 'APPID',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 100
            },
            {
                text: "出庫庫房",
                dataIndex: 'FRWH_NAME',
                width: 140
            },
            {
                text: "入庫庫房",
                dataIndex: 'TOWH_NAME',
                width: 140
            },
            {
                text: "調整類別",
                dataIndex: 'APPLY_KIND',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }

                    chkBtnStatus();
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                T2Load();

                if (T1LastRec == null)
                    Ext.getCmp('btnAdd2').setDisabled(true);
            }
        }
    });

    function chkBtnStatus() {
        // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
        if (T1LastRec != null) {

            var canEdit = true;
            var canSubmit = true;
            var selection = T1Grid.getSelection();
            if (selection.length) {
                if (selection.length > 1)
                    canEdit = false; // 勾選資料大於1筆則不可使用編輯功能
                $.map(selection, function (item, key) {
                    if (item.get('FLOWID') != '未過帳') {
                        canEdit = false; // 有任一筆不是未過帳則不可過帳和編輯
                        canSubmit = false;
                    }

                })

            }
            if (canEdit) {
                Ext.getCmp('btnUpdate').setDisabled(false);
                Ext.getCmp('btnDel').setDisabled(false);

                Ext.getCmp('btnAdd2').setDisabled(false);
            }
            else {
                Ext.getCmp('btnUpdate').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(true);

                Ext.getCmp('btnAdd2').setDisabled(true);
            }

            if (canSubmit)
                Ext.getCmp('btnSubmit').setDisabled(false);
            else
                Ext.getCmp('btnSubmit').setDisabled(true);

            Ext.getCmp('btnUpdate2').setDisabled(true);
            Ext.getCmp('btnDel2').setDisabled(true);
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
            Ext.getCmp('btnSubmit').setDisabled(true);
            Ext.getCmp('btnAdd2').setDisabled(true);
        }
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        chkBtnStatus();
        if (T1LastRec != null) {
            viewport.down('#form').expand();
            Tabs.setActiveTab('Form');
            var currentTab = Tabs.getActiveTab();
            currentTab.setTitle('瀏覽');

            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("APPLY_KIND").setValue(T1LastRec.data["APPLY_KIND"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("WH_KIND").setValue(T1LastRec.data["WH_KIND"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

        }

    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AA0155/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AA0155/DetailUpdate';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0155/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
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
                        });
                    }
                }
            }
        ]
    });

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH_NAME"]);
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

        }
        else {
            u = f.findField('MMCODE');

            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
        }
        f.findField('x').setValue(x);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "轉換數量",
                dataIndex: 'APPQTY',
                width: 80,
                align: 'right',
            },
            {
                text: "單價",
                dataIndex: 'UP',
                width: 70,
                align: 'right',
            },
            {
                text: "金額",
                dataIndex: 'AMT',
                width: 70,
                align: 'right',
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {

                        var canAdd = true;
                        var canEdit = true;
                        var selection = T2Grid.getSelection();
                        if (selection.length) {
                            if (selection.length > 1)
                                canEdit = false; // 勾選資料大於1筆則不可使用編輯功能
                        }
                        else
                            canEdit = false;
                        if (T1LastRec.data["FLOWID"] != '未過帳')
                            canAdd = false; // 不是未過帳則不可使用新增功能

                        if (canAdd) {
                            if (canEdit) {
                                Ext.getCmp('btnUpdate2').setDisabled(false);
                                Ext.getCmp('btnDel2').setDisabled(false);
                            }
                            else {
                                Ext.getCmp('btnUpdate2').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(true);
                            }
                            Ext.getCmp('btnAdd2').setDisabled(false);
                        }
                        else {
                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            Ext.getCmp('btnAdd2').setDisabled(true);
                        }

                        Ext.getCmp('btnUpdate').setDisabled(true);         
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnSubmit').setDisabled(true);

                        if (T1Set === '')
                            setFormT2a();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('UP').setValue(T2LastRec.data["UP"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }

    function T1Cleanup() {
        FrwhLoadF('');
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        f.findField('MAT_CLASS').setReadOnly(true);
        f.findField('TOWH').setReadOnly(true);
        f.findField('FRWH').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
    }

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            f.findField("MMNAME_C").setValue(args.MMNAME_C);
            f.findField("MMNAME_E").setValue(args.MMNAME_E);
            f.findField("UP").setValue(args.M_CONTPRICE);
            f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
        }
    }

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0155/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                return {
                    DOCNO: T1LastRec.data["DOCNO"]
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                    f.findField("UP").setValue(r.get('M_CONTPRICE'));
                }
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60,
            width: 300,
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '異動單號',
                name: 'DOCNO',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '出庫庫房',
                name: 'FRWH',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
                    mmCodeCombo
                ]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '進貨單價',
                name: 'UP',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textfield',
                fieldLabel: '轉換數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true,
                allowBlank: false,
                fieldCls: 'required',
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                                //T1Set = '';
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T2Cleanup
            }
        ]
    });

    function T2Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
    }

    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.DN);
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                    }

                    T2Cleanup();
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
                            break;
                    }
                }
            });
        }
    }

    var Tabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
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
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [Tabs]
            }
        ]
    });
    UserInfoLoad();
    //TowhLoad();
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
});