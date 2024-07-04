Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0006/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材非庫備申請";
    var reportUrl = '/Report/A/AA0013.aspx';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP2 = ["1", "2"];
    var T6GetExcel = '../../../api/AB0006/Excel';
    var getDocAppAmout = '/api/AB0003/GetDocAppAmout';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var st_pkdocno = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2,
                    p1: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetDocnopkCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_pknote = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1F2,
                    p1: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetDocpknoteCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        }
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_apply_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetApplyKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_reason = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetReasonCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位

    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
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
                xtype: 'combo',
                fieldLabel: '申請單號',
                name: 'P0',
                id: 'P0',
                store: st_docno,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                vtype: 'dateRange',
                dateRange: { end: 'D1' },
                padding: '0 4 0 4'
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'D0' },
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單狀態',
                name: 'P2',
                id: 'P2',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單分類',
                name: 'P3',
                id: 'P3',
                store: st_apply_kind,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                var f = this.up('form').getForm();
                if (f.findField('P0').getValue() == '' &&
                    f.findField('D0').rawValue == '' &&
                    f.findField('D1').rawValue == '' &&
                    f.findField('P2').getForm() == '' &&
                    f.findField('P3').getForm() == '') {
                    Ext.Msg.alert('提醒', '至少需輸入1個條件');
                }
                else {
                    T1Load();
                }
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
                T1QueryForm.getForm().findField('P2').setValue(arrP2);
            }
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'DOCTYPE', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'USEID', type: 'string' },
            { name: 'USEDEPT', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'PURCHASENO', type: 'string' },
            { name: 'SUPPLYNO', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'MAT_CLASS_N', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APPLY_KIND_N', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'APPDEPT_NAME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'MR1', type: 'string' },
            { name: 'MR2', type: 'string' },
            { name: 'MR3', type: 'string' },
            { name: 'MR4', type: 'string' },
            { name: 'EXT', type: 'string' },
            { name: 'APP_AMOUT', type: 'string' }
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
            url: '/api/AB0006/AllM',
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
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    d0: T1QueryForm.getForm().findField('D0').rawValue,
                    d1: T1QueryForm.getForm().findField('D1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue()
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
                    T1Set = '/api/AB0006/CreateM'; // AB0006Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0006/UpdateM';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
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
                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0006/DeleteM',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:資料刪除成功');
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
                itemId: 'apply', text: '送核撥', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
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
                        Ext.MessageBox.confirm('送核撥', '是否確定送核撥？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0006/Apply',
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
                                            msglabel('訊息區:送核撥成功');
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
                itemId: 'savepk', text: '套餐儲存', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin3();
                }
            },
            {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
                    showReport();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/ME_DOCM.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("DOCNO");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('DOCNO_D').setValue('系統自編');
            f.findField('FLOWID_N').setValue('申請中');
            f.findField('APPTIME_T').setValue(st_getlogininfo.getAt(0).get('TODAY'));
            f.findField('FRWH_N').setValue(st_getlogininfo.getAt(0).get('CENTER_WHNAME'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('APPDEPT_NAME').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('MR4');
            f.findField('APPLY_KIND_N').setValue('常態申請');
            f.findField('MAT_CLASS').setValue(st_matclass.getAt(0).get('VALUE'));
            //f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            if (st_towhcombo.getCount() > 0) {
                f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            }
            else {
                viewport.down('#form').collapse();
                Ext.MessageBox.alert('錯誤', '查無庫房資料,不得新增');
            }
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('TOWH').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('APPLY_NOTE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

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
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI',
            listeners: {

            }
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 180
        }, {
            text: "類別",
            dataIndex: 'APPLY_KIND_N',
            width: 80
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 80
        }, {
            text: "申請人員",
            dataIndex: 'APP_NAME',
            width: 80
        }, {
            text: "申請部門",
            dataIndex: 'APPDEPT_NAME',
            width: 80
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_N',
            width: 120
        }, {
            text: "入庫庫房",
            dataIndex: 'TOWH_N',
            width: 120
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS_N',
            width: 100
        }, {
            text: "分機號碼",
            dataIndex: 'EXT',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T1Rec = records.length;
                T1LastRec = records[0];

                if (T1Grid.getSelection().length > 1) {
                    T2Grid.down('#add').setDisabled(true);
                    T2Grid.down('#edit').setDisabled(true);
                    T2Grid.down('#delete').setDisabled(true);
                    T2Grid.down('#getpk').setDisabled(true);
                    T2Grid.down('#getsave').setDisabled(true);
                    T2Grid.down('#getexport').setDisabled(true);
                    T2Grid.down('#addMulti').setDisabled(true);
                }
                else
                    setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#savepk').setDisabled(T1Rec === 0);
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        T2Grid.down('#addMulti').setDisabled(T1Rec === 0);
        T2Grid.down('#getpk').setDisabled(T1Rec === 0);
        T2Grid.down('#getsave').setDisabled(T1Rec === 0);
        T2Grid.down('#getexport').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('MAT_CLASS').setReadOnly(true);
            f.findField('TOWH').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(T1F1);
            T1F2 = f.findField('MAT_CLASS').getValue();
            T1F3 = f.findField('FLOWID').getValue();
            T1F4 = f.findField('TOWH').getValue();
            T1F5 = f.findField('APPDEPT').getValue();
            T1F6 = f.findField('APPDEPT_NAME').getValue();
            T1F7 = f.findField('MAT_CLASS_N').getValue();
            T3Form.getForm().findField('NOTE').setValue(T1F6 + T1F7 + '套餐');
            st_pkdocno.load();
            st_pknote.load();
            if (T1F3 == '1') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#apply').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
                T2Grid.down('#addMulti').setDisabled(false);
                T2Grid.down('#getpk').setDisabled(false);
                T2Grid.down('#getsave').setDisabled(false);
                T2Grid.down('#getexport').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                T1Grid.down('#apply').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
                T2Grid.down('#addMulti').setDisabled(true);
                T2Grid.down('#getpk').setDisabled(true);
                T2Grid.down('#getsave').setDisabled(true);
                T2Grid.down('#getexport').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
        T2Query.getForm().reset();
        T2Load(true);

    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
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
        items: [{
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCTYPE',
            xtype: 'hidden'
        }, {
            name: 'FLOWID',
            xtype: 'hidden'
        }, {
            name: 'FRWH',
            xtype: 'hidden'
        }, {
            name: 'APPLY_KIND',
            xtype: 'hidden'
        }, {
            name: 'APPID',
            xtype: 'hidden'
        }, {
            name: 'APPDEPT',
            xtype: 'hidden'
        }, {
            name: 'USEID',
            xtype: 'hidden'
        }, {
            name: 'USEDEPT',
            xtype: 'hidden'
        }, {
            name: 'APPTIME',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'MAT_CLASS_N',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DOCNO_D'
        }, {
            xtype: 'displayfield',
            fieldLabel: '類別',
            name: 'APPLY_KIND_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'FLOWID_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請人員',
            name: 'APP_NAME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請部門',
            name: 'APPDEPT_NAME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請時間',
            name: 'APPTIME_T'
        }, {
            xtype: 'displayfield',
            fieldLabel: '出庫庫房',
            name: 'FRWH_N'
        }, {
            xtype: 'combo',
            fieldLabel: '入庫庫房',
            name: 'TOWH',
            store: st_towhcombo,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS',
            store: st_matclass,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE',
            enforceMaxLength: true,
            maxLength: 100,
            height: 200,
            readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    /*
                                        if (this.up('form').getForm().findField('WH_NO').getValue() == ''
                                        ) //&& this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                                            Ext.Msg.alert('提醒', '至少需輸入');
                                        else {*/
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
                /*else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');*/
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
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
                            T1QueryForm.getForm().reset();
                            var v = action.result.etts[0];
                            T1QueryForm.getForm().findField('P0').setValue(v.DOCNO);
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料核撥成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    T1Cleanup();
                    T1Load();
                    TATabs.setActiveTab('Query');
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
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
        T2Cleanup();
        T1QueryForm.getForm().findField('P2').setValue(arrP2);
        TATabs.setActiveTab('Query');
    }

    function showReport() {
        if (!win) {
            var np = {
                p5: T1F1,
                Action: 4
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p5=' + np.p5 + '&Action=' + np.Action + '&Order=mmcode" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0006/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [
            T2QueryMMCode, {
                xtype: 'button',
                text: '查詢',
                handler: T2Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }, {
                xtype: 'displayfield',
                fieldLabel: '申請總金額',
                padding: '0 0 0 8',
                labelAlign: 'right',
                name: 'APP_AMOUT',
                labelWidth: 70,
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        return '<span style="color:red">' + val + '</span>';
                    }
                }
            }]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'APP_AMT', type: 'string' },
            { name: 'HIGH_QTY', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/AllD',
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
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful)
                    reCalAppAmout();
            }
        }
    });
    function T2Load(moveFirst) {
        try {
            if (moveFirst) {
                T2Tool.moveFirst();
            } else {
                T2Store.load({
                    params: {
                        start: 0
                    }
                });
            }
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }
    //T1Load();

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            T2FormMMCode.doQuery();
            var func = function () {
                var record = T2FormMMCode.store.getAt(0);
                T2FormMMCode.select(record);
                T2FormMMCode.fireEvent('select', this, record);
                T2FormMMCode.store.un('load', func);
            };
            T2FormMMCode.store.on('load', func);
        }
    }
    var T2FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0006/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'AVG_PRICE', 'INV_QTY', 'AVG_APLQTY', 'HIGH_QTY', 'TOT_APVQTY', 'TOT_DISTUN', 'PFILE_ID'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('TOWH').getValue(),
                p3: T1Form.getForm().findField('DOCNO').getValue(),
                p4: T1Form.getForm().findField('FRWH').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('AVG_PRICE').setValue(r.get('AVG_PRICE'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));
                T2Form.getForm().findField('TOT_APVQTY').setValue(r.get('TOT_APVQTY'));
                T2Form.getForm().findField('TOT_DISTUN').setValue(r.get('TOT_DISTUN'));
                T2Form.getForm().findField('PFILE_ID').setValue(r.get('PFILE_ID'));

                if (r.get('WHMM_VALID') == 'Y') {
                    Ext.getCmp('WHMM_VALID').hide();
                } else {
                    Ext.getCmp('WHMM_VALID').show();
                }

                if (T2Form.getForm().findField('APPQTY').getValue()) {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parAVG_PRICE = parseFloat(T2Form.getForm().findField('AVG_PRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parAVG_PRICE, parAPPQTY));
                }
            }
        }
    });
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        autoScroll: true,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            name: 'STAT',
            xtype: 'hidden'
        },
        {
            xtype: 'container',
            layout: 'hbox',
            //padding: '0 7 7 0',
            items: [
                T2FormMMCode,
                {
                    xtype: 'button',
                    itemId: 'btnMmcode',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T2Form.getForm();
                        if (!f.findField("MMCODE").readOnly) {
                            popMmcodeForm_AB0004(viewport, '/api/AB0006/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                                WH_NO: T1Form.getForm().findField('TOWH').getValue()
                            }, setMmcode);
                        }
                    }
                },

            ]
        }, {
            xtype: 'displayfield',
            value: '<spam style="color:red">本庫房無法申領此院內碼</span>',
            name: 'WHMM_VALID',
            id: 'WHMM_VALID',
            hidden: true,
        }
            , {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '計量單位',
            name: 'BASE_UNIT'
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨單價',
            name: 'M_CONTPRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存平均單價',
            name: 'AVG_PRICE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '中央庫房庫存量',
            name: 'INV_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '安全存量',
            name: 'SAFE_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請金額',
            name: 'APP_AMT'
        }, {
            xtype: 'displayfield',
            fieldLabel: '最小撥補量',
            name: 'TOT_DISTUN'
        }, {
            fieldLabel: '申請數量',
            name: 'APPQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function () {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parAVG_PRICE = parseFloat(T2Form.getForm().findField('AVG_PRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parAVG_PRICE, parAPPQTY));
                }
            }
        }, {
            xtype: 'label',
            text: '申請量必須為最小撥補量的倍數',
            style: 'color: #ff0000;'
        }, {
            xtype: 'displayfield',
            fieldLabel: '月基準量',
            name: 'HIGH_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '本月累計核撥量',
            name: 'TOT_APVQTY'
        }, {
            xtype: 'combo',
            fieldLabel: '超量原因',
            name: 'GTAPL_RESON',
            store: st_reason,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            readOnly: true,
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APLYITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 50,
            height: 200,
            readOnly: true
        }, {
            xtype: 'imagegrid',
            fieldLabel: '附加圖片',
            name: 'PFILE_ID',
            width: 300,
            height: 200
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                var isSub = true;
                if (this.up('form').getForm().findField('APPQTY').getValue() == '0') {
                    Ext.Msg.alert('提醒', '申請數量不可為0');
                    isSub = false;
                }
                else {
                    var highqty = 9999;
                    if (this.up('form').getForm().findField('HIGH_QTY').getValue() != null) {
                        highqty = Number(this.up('form').getForm().findField('HIGH_QTY').getValue());
                    }
                    var tot_apvqty = 0;
                    if (this.up('form').getForm().findField('TOT_APVQTY').getValue() != null) {
                        tot_apvqty = Number(this.up('form').getForm().findField('TOT_APVQTY').getValue());
                    }
                    var appqty = 0;
                    if (this.up('form').getForm().findField('APPQTY').getValue() != null) {
                        appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
                    }
                    var minodrqty = 0;
                    if (this.up('form').getForm().findField('TOT_DISTUN').getValue() != null) {
                        minodrqty = Number(this.up('form').getForm().findField('TOT_DISTUN').getValue());
                    }
                    if ((tot_apvqty + appqty) > highqty && this.up('form').getForm().findField('GTAPL_RESON').getValue() == null) {
                        Ext.Msg.alert('提醒', '本月累計核撥量加本次申請量超過月基準量，請敘明原因!');
                        isSub = false;
                    }
                    if ((appqty % minodrqty) != 0) {
                        Ext.Msg.alert('提醒', '申請量必須為最小撥補量的倍數!');
                        isSub = false;
                    }
                }
                if (isSub) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    }
                    );
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                T2Load(true);
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
                url: T11Set,
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
                            reCalAppAmout();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            reCalAppAmout();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            break;
                    }
                    if (f2.findField("x").getValue() == "I") {
                        T11Set = '../../../api/AB0006/CreateD';
                        setFormT2('I', '新增');
                    }
                    else {
                        if (f2.findField("x").getValue() == "U") {
                            T2Load(false);
                        } else {
                            T2Load(true);
                        }
                        T2Cleanup();
                    }
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
        f.findField('APLYITEM_NOTE').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        T2Form.down('#btnMmcode').setVisible(false);
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
                    T11Set = '../../../api/AB0006/CreateD';
                    Ext.getCmp('WHMM_VALID').hide();
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '../../../api/AB0006/UpdateD';
                    Ext.getCmp('WHMM_VALID').hide();
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
                        let seq = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('SEQ') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0006/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            msglabel('訊息區:刪除成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load(true);
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        } else
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
                itemId: 'getpk', text: '套餐轉入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin4();
                }
            },
            {
                itemId: 'getsave', text: '低於安全存量轉入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin5();
                }
            },
            {
                itemId: 'getexport', text: '匯入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    showWin6();
                }
            },
            {
                itemId: 'addMulti', text: '多筆新增', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    popWinForm();
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
            T2Form.down('#btnMmcode').setVisible(true);
            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('APLYITEM_NOTE').setReadOnly(false);
        f2.findField('GTAPL_RESON').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        //autoScroll: true,
        cls: 'T2',
        //defaults: {
        //    layout: 'fit'
        //},
        dockedItems: [{
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "建立人員",
            dataIndex: 'CREATE_USER_NAME',
            width: 90,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        //}, {
        //    text: "進貨單價",
        //    dataIndex: 'M_CONTPRICE',
        //    style: 'text-align:left',
        //    width: 80, align: 'right'
        }, {
            text: "庫存平均單價",
            dataIndex: 'AVG_PRICE',
            width: 100, align: 'right'
        }, {
            text: "中央庫房庫存量",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
            text: "安全存量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "申請金額",
            dataIndex: 'APP_AMT',
            style: 'text-align:left',
            width: 140, align: 'right',
            renderer: function (val, meta, record) {
                // 申請金額 = 庫存平均單價 * 申請數量
                var parAVG_PRICE = parseFloat(record.data['AVG_PRICE']);
                var parAPPQTY = parseFloat(record.data['APPQTY']);
                return accMul(parAVG_PRICE, parAPPQTY);
            }
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "預計核撥量",
            dataIndex: 'EXPT_DISTQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "實際核撥量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "點收數量",
            dataIndex: 'ACKQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "送核撥時間",
            dataIndex: 'APL_CONTIME',
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
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                        TATabs.setActiveTab('Form');
                    }
                }
            },

            selectionchange: function (model, records) {
                viewport.down('#form').expand();
                T2Rec = records.length;
                T2LastRec = records[0];

                if (T2Grid.getSelection().length > 1)
                    T2Grid.down('#edit').setDisabled(true);
                else
                    setFormT2a();
                //viewport.down('#form').addCls('T1b');
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
            //f.findField('DATA_SEQ_O').setValue(T2LastRec.get('DATA_SEQ'));
            //var u = f.findField('ID');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            if (T1F3 === '1') {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
            }
            else {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }

        }
        else {
            T2Form.getForm().reset();
        }
    }

    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'displayfield',
        autoScroll: true,
        items: [
            {
                xtype: 'textareafield',
                fieldLabel: '菜單說明',
                name: 'NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                height: 200
            }
        ],
        buttons: [{
            itemId: 'T3Submit', text: '儲存', handler: function () {
                if (this.up('form').getForm().isValid()) {
                    Ext.MessageBox.confirm('儲存套餐', '是否確定儲存套餐?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0006/Savepk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1F1,
                                    MAT_CLASS: T1F2,
                                    NOTE: T3Form.getForm().findField('NOTE').getValue()
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        hideWin3();
                                        Ext.MessageBox.alert('訊息', '套餐儲存成功');
                                        T2Store.removeAll();
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
            itemId: 'cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'TOT_DISTUN', type: 'string' }
        ]
    });
    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetPackD',
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
                    p0: T4Query.getForm().findField('P0').getValue(),
                    p1: T4Query.getForm().findField('P1').getValue(),
                    p2: T1F2,
                    p3: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }
    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'combo',
            fieldLabel: '套餐單號',
            name: 'P0',
            store: st_pkdocno,
            queryMode: 'local',
            displayField: 'TEXT',
            valueField: 'VALUE',
            labelAlign: 'right',
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'combo',
            fieldLabel: '套餐說明',
            name: 'P1',
            store: st_pknote,
            queryMode: 'local',
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            labelAlign: 'right',
            width: '500px',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            anyMatch: true,
            listeners: {
                select: function (c, r, eo) {
                    Ext.getCmp('T4btn1').setDisabled(false);
                    Ext.getCmp('T4btn2').setDisabled(false);
                }
            }
        }, {
            xtype: 'button',
            text: '查詢',
            id: 'T4btn1',
            disabled: true,
            handler: function () {
                T4Load();
                msglabel('訊息區:');
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }, {
            xtype: 'button', text: '轉入',
            id: 'T4btn2',
            disabled: true,
            handler: function () {
                var selection = T4Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    var msg = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.APPQTY % selection[i].data.TOT_DISTUN != 0) {
                            if (msg != '') {
                                msg += '<br>';
                            }
                            msg += ('院內碼:' + selection[i].data.MMCODE + ' 申請量:' + selection[i].data.APPQTY + ' 最小撥補量:' + selection[i].data.TOT_DISTUN);
                        }
                    }
                    if (msg != '') {
                        msg = '<span style="color:red">申請量須為最小撥補量的倍數，請至套餐建置修改</span><br>' + msg;
                        Ext.Msg.alert('提醒', msg);
                        return;
                    }

                    let docno = '';
                    let mmcode = '';
                    let appqty = '';
                    //selection.map(item => {
                    //    name += '「' + item.get('MMCODE') + '」<br>';
                    //    docno += T1F1 + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //    appqty += item.get('APPQTY') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('MMCODE') + '」<br>';
                        docno += T1F1 + ',';
                        mmcode += item.get('MMCODE') + ',';
                        appqty += item.get('APPQTY') + ',';
                    })
                    Ext.MessageBox.confirm('轉入', '是否確定轉入？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0006/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        hideWin4();
                                        //Ext.MessageBox.alert('訊息', '轉入成功');
                                        msglabel('訊息區:轉入成功');
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load(true);
                                    }
                                    else {
                                        hideWin4();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin4();
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

    function T4Cleanup() {
        T4Query.getForm().reset();
        T4Load();
        msglabel('訊息區:');
        Ext.getCmp('T4btn1').setDisabled(true);
        Ext.getCmp('T4btn2').setDisabled(true);
    }
    var T4Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T4Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
            }, {
                text: "",
                dataIndex: 'WHMM_VALID',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data.WHMM_VALID == 'N') {
                        return '<span style="color: red">本庫房不可申領</span>';
                    }
                },
            }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 80, align: 'right'
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "最小撥補量",
            dataIndex: 'TOT_DISTUN',
            style: 'text-align:left',
            width: 80, align: 'right'
        },{
            header: "",
            flex: 1
        }
        ]
    });

    var T5Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0006/GetSaveD',
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
                    p2: T1F2,
                    p3: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T5Load() {
        T5Store.load({
            params: {
                start: 0
            }
        });
    }
    var T5Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'button', text: '轉入',
            id: 'T5btn2',
            handler: function () {
                var selection = T5Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let docno = '';
                    let mmcode = '';
                    let appqty = '';
                    //selection.map(item => {
                    //    name += '「' + item.get('MMCODE') + '」<br>';
                    //    docno += T1F1 + ',';
                    //    mmcode += item.get('MMCODE') + ',';
                    //    appqty += item.get('APPQTY') + ',';
                    //});
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('MMCODE') + '」<br>';
                        docno += T1F1 + ',';
                        mmcode += item.get('MMCODE') + ',';
                        appqty += item.get('APPQTY') + ',';
                    })
                    Ext.MessageBox.confirm('轉入', '是否確定轉入？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AB0006/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.MessageBox.alert('訊息', '轉入成功');
                                        msglabel('訊息區:轉入成功');
                                        hideWin5();
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load(true);
                                    }
                                    else {
                                        hideWin5();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin5();
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

    function T5Cleanup() {
        T5Query.getForm().reset();
        T5Load();
        msglabel('訊息區:');
    }
    var T5Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T5Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T5Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
            }, {
                text: "",
                dataIndex: 'WHMM_VALID',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data.WHMM_VALID == 'N') {
                        return '<span style="color: red">本庫房不可申領</span>';
                    }
                },
            }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 80, align: 'right'
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
            }, {
                text: "超量原因",
                dataIndex: 'GTAPL_RESON_N',
                style: 'text-align:left',
                width: 150, align: 'right'
            }, {
            header: "",
            flex: 1
        }
        ]
    });

    var T6Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            //url: '/api/AB0006/GetSaveD',
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
                    p2: T1F2,
                    p3: T1F5
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T6Load() {
        T6Store.load({
            params: {
                start: 0
            }
        });
    }
    var T6Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 't6export',
                name: 'T6insert',
                text: '範本', handler: function () {
                    var p = new Array();
                    //p.push({ name: 'FN', value: today + '_新合約品項批次更新_物料主檔.xls' });
                    PostForm(T6GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T6send',
                id: 'T6send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T6insert').setDisabled(true);
                        T6Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T6send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            formData.append("matclass", T1F2);
                            formData.append("docno", T1F1);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AB0006/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T6Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T6insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T6Store.loadData(data.etts, false);
                                        IsSend = true;
                                        T6Grid.columns[1].setVisible(true);
                                        //T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T6insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T6insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'T6insert',
                name: 'T6insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AB0006/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T6Store.data.items, 'data')),
                            docno: T1F1
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T6insert').setDisabled(true);
                                T6Store.removeAll();
                                T6Grid.columns[1].setVisible(false);
                                T2Load(true);
                            }
                            myMask.hide();
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
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin6();
                }
            }
        ]
    });

    function T6Cleanup() {
        T6Query.getForm().reset();
        //T6Load();
        msglabel('訊息區:');
    }
    var T6Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T6Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Query]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        //selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            hidden: true,
            width: 250
        }, {
            dataIndex: 'DOCNO',
            hidden: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        },
        //    {
        //    text: "中文品名",
        //    dataIndex: 'MMNAME_C',
        //    width: 120,
        //    sortable: true
        //}, {
        //    text: "英文品名",
        //    dataIndex: 'MMNAME_E',
        //    width: 150,
        //    sortable: true
        //}, {
        //    text: "單位",
        //    dataIndex: 'BASE_UNIT',
        //    width: 50,
        //    sortable: true
        //}, {
        //    text: "庫存單價",
        //    dataIndex: 'AVG_PRICE',
        //    width: 80, align: 'right'
        //    },
        {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        },
        //{
        //text: "超量原因",
        //dataIndex: 'GTAPL_RESON_N',
        //style: 'text-align:left',
        //width: 150, align: 'right'
        //}, 
        {
            header: "",
            flex: 1
        }
        ]
    });
    //view 
    var TATabs = Ext.widget('tabpanel', {
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
            items: [T1QueryForm]
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
                        // items: [TATabs]
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
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '套餐儲存',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
            //viewport.down('#form').collapse();
        }
    }

    var win4;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '套餐',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T4Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin4() {
        if (win4.hidden) {
            T4Cleanup();
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
            T4Cleanup();
            //viewport.down('#form').collapse();
        }
    }

    var win5;
    if (!win5) {
        win5 = Ext.widget('window', {
            title: '低於安全存量',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T5Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin5() {
        if (win5.hidden) {
            T5Cleanup();
            T5Load();
            win5.show();
        }
    }
    function hideWin5() {
        if (!win5.hidden) {
            win5.hide();
            T5Cleanup();
            //viewport.down('#form').collapse();
        }
    }

    var win6;
    if (!win6) {
        win6 = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T6Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin6() {
        if (win6.hidden) {
            T6Cleanup();
            T6Store.removeAll();
            //T6Load();
            win6.show();
        }
    }
    function hideWin6() {
        if (!win6.hidden) {
            win6.hide();
            T6Cleanup();
            //viewport.down('#form').collapse();
        }
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P2').setValue(arrP2);

    // 重算當前申請單的申請總金額
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

    //#region 多筆新增
    // 供彈跳視窗內的呼叫以關閉彈跳視窗
    var callableWin = null;
    popWinForm = function () {
        var title = '';
        title = '衛材非庫備 申請單號：' + T1LastRec.data.DOCNO;

        var strUrl = "AB0003_Form?docno=" + T1LastRec.data.DOCNO + "&mStoreid=0&matClass=" + T1LastRec.data.MAT_CLASS;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                        T2Load(true);
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion
});
