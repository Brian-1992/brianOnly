
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);
Ext.onReady(function () {
    var T1Get = '/api/AB0127/AllM'; // 查詢
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "藥品申請";
    var reportUrl = '/Report/A/AB0118.aspx';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP2 = []; // 狀態預設值
    var T6GetExcel = '../../../api/AB0127/Excel';
    var detailExport = '../../../api/AB0127/DetailExcel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_pkdocno = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: '01',
                    p1: T1F4,
                    p2: T1F9
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0127/GetDocnopkCombo',
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
                    p0: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0127/GetDocpknoteCombo',
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
            url: '/api/AB0127/GetFlowidCombo',
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
            url: '/api/AB0127/GetReasonCombo',
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
            url: '/api/AB0127/GetTowhCombo',
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
            url: '/api/AB0127/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, eOpts) {
                if (store.getAt(0).get('IS_GRADE1') == 'Y' && store.getAt(0).get('HOSP_CODE') == '805') {
                    Ext.getCmp('checkInvqty').show();
                }
            }
        },
        autoLoad: true
    });

    var st_isarmy = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0127/GetIsArmyCombo',
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
        bodyPadding: '7 7 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: '申請單',
                name: 'P0',
                id: 'P0'
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
                padding: '0 4 0 4',
                value: new Date()
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
                xtype: 'checkbox',
                name: 'P3',
                width: 130,
                boxLabel: '補藥轉單',
                inputValue: '1',
                checked: false,
                padding: '0 4 0 9'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                var f = this.up('form').getForm();
                if (f.findField('P0').getValue() == '' &&
                    f.findField('D0').rawValue == '' &&
                    f.findField('D1').rawValue == '' &&
                    f.findField('P2').getValue() == ''
                ) {
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
            { name: 'TOWH', type: 'string' },
            { name: 'PURCHASENO', type: 'string' },
            { name: 'SUPPLYNO', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'APPDEPT_NAME', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'MR1', type: 'string' },
            { name: 'MR2', type: 'string' },
            { name: 'MR3', type: 'string' },
            { name: 'MR4', type: 'string' },
            { name: 'EXT', type: 'string' },
            { name: 'SRCDOCNO', type: 'string' },
            { name: 'ISARMY', type: 'string' },
            { name: 'ISARMY_N', type: 'string' },
            { name: 'APPUNA', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_T', type: 'string' }
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
            url: T1Get,
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
                    p3: T1QueryForm.getForm().findField('P3').rawValue
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
                    T1Set = '/api/AB0127/CreateM';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0127/UpdateM';
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
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0127/DeleteM',
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
                itemId: 'apply', text: '提出申請', disabled: true, handler: function () {
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
                        //20230913增加 桃園需求:檢查出貨單位但不卡死
                        Ext.Ajax.request({
                            url: '/api/AB0127/CheckUnitrateFlg',
                            method: reqVal_p,
                            params: {
                                DOCNO: docno
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    if (data.msg != '') {
                                        Ext.MessageBox.confirm('提醒', '申請單號(' + data.msg + ')有品項申請量不為出貨單位的倍數，是否仍要提出申請?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/AB0127/Apply',
                                                    method: reqVal_p,
                                                    params: {
                                                        DOCNO: docno
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            T2Store.removeAll();
                                                            T1Grid.getSelectionModel().deselectAll();
                                                            T1QueryForm.getForm().findField('P0').setValue(''); //提出申請後單號清空才查詢
                                                            T1Load();
                                                            msglabel('訊息區:提出申請成功');
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
                                    } else {
                                        Ext.MessageBox.confirm('提出申請', '是否確定提出申請？單號如下<br>' + name, function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/AB0127/Apply',
                                                    method: reqVal_p,
                                                    params: {
                                                        DOCNO: docno
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            T2Store.removeAll();
                                                            T1Grid.getSelectionModel().deselectAll();
                                                            T1QueryForm.getForm().findField('P0').setValue(''); //提出申請後單號清空才查詢
                                                            T1Load();
                                                            msglabel('訊息區:提出申請成功');
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
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                }
            },
            {
                itemId: 'cancel', text: '取消送申請', disabled: true, handler: function () {
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
                        Ext.MessageBox.confirm('取消送申請', '是否確定取消送申請？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0127/Cancel',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:取消送申請申請成功');
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
            },
            {
                itemId: 'savepk', text: '套餐儲存', disabled: true, hidden: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin3();
                }
            },
            {
                itemId: 'print', text: '列印', disabled: true, hidden: false, handler: function () {
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
                        showReport(docno);
                    }
                }
            },
            {
                itemId: 'masterExcel', text: '匯出', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                        return;
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        list.push(selection[i].data);
                    }

                    var p = new Array();
                    p.push({ name: 'docnos', value: Ext.util.JSON.encode(list) });
                    PostForm('/api/AB0127/MasterExcel', p);
                }
            },
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
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('APPDEPT').setValue(st_getlogininfo.getAt(0).get('INID'));
            f.findField('APPDEPT_NAME').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('ISARMY').setValue(st_isarmy.getAt(1).get('VALUE'));
            if (st_towhcombo.getCount() > 0) {
                f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            }
            else {
                viewport.down('#form').collapse();
                Ext.MessageBox.alert('錯誤', '查無庫房資料,不得新增');
            }
            f.findField('TOWH').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('ISARMY').setReadOnly(false);
        f.findField('APPUNA').setReadOnly(false);
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
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 170
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 120
        }, {
            text: "申請人員",
            dataIndex: 'APP_NAME',
            width: 100
        }, {
            text: "申請日期",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "入庫庫房",
            dataIndex: 'TOWH_N',
            width: 120
        }, {
            text: "軍民別",
            dataIndex: 'ISARMY_N',
            width: 100
        }, {
            text: "申請人姓名",
            dataIndex: 'APPUNA',
            width: 100
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID_T',
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

                T1Grid.down('#masterExcel').setDisabled(true);
                if (T1Grid.getSelection().length > 0) {
                    T1Grid.down('#masterExcel').setDisabled(false);
                }

                if (T1Grid.getSelection().length > 1) {
                    T2Grid.down('#add').setDisabled(true);
                    T2Grid.down('#edit').setDisabled(true);
                    T2Grid.down('#delete').setDisabled(true);
                    T2Grid.down('#getpk').setDisabled(true);
                    T2Grid.down('#getsave').setDisabled(true);
                    T2Grid.down('#getexport').setDisabled(true);
                    Ext.getCmp('btnGetInvqty').setDisabled(true);
                    Ext.getCmp('btnGetUseqty').setDisabled(true);//控制檢查庫存按鈕
                }
                else {
                    setFormT1a();
                }

                if (T1Grid.getSelection().length == 1 && T1LastRec.data.DOCNO != null) {
                    Ext.getCmp('detailExport').setDisabled(false);
                }
                else {
                    Ext.getCmp('detailExport').setDisabled(true);
                }
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#cancel').setDisabled(T1Rec === 0);
        T1Grid.down('#savepk').setDisabled(T1Rec === 0);
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        T2Grid.down('#getpk').setDisabled(T1Rec === 0);
        T2Grid.down('#getsave').setDisabled(T1Rec === 0);
        T2Grid.down('#getexport').setDisabled(T1Rec === 0);

        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            T3Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('TOWH').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(T1F1);
            T1F3 = f.findField('FLOWID').getValue();
            T1F4 = f.findField('TOWH').getValue();
            T1F7 = f.findField('APPDEPT').getValue();
            T1F6 = f.findField('APPDEPT_NAME').getValue();
            T1F9 = f.findField('DOCTYPE').getValue();
            T3Form.getForm().findField('NOTE').setValue(T1F6 + '藥品套餐');
            st_pkdocno.load();
            st_pknote.load();
            if (T1F3 == '0101' || T1F3 == '0601') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#apply').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
                T2Grid.down('#getpk').setDisabled(false);
                T2Grid.down('#getsave').setDisabled(false);
                T2Grid.down('#getexport').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                T1Grid.down('#apply').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
                T2Grid.down('#getpk').setDisabled(true);
                T2Grid.down('#getsave').setDisabled(true);
                T2Grid.down('#getexport').setDisabled(true);
            }
            if (T1F3 == '0111' || T1F3 == '0611') {
                T1Grid.down('#cancel').setDisabled(false);
            }
            else {
                T1Grid.down('#cancel').setDisabled(true);
            }
            if (T1F3 == '0111' || T1F3 == '0611') {
                T1Grid.down('#print').setDisabled(false);
            }
            else {
                T1Grid.down('#print').setDisabled(true);
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
        bodyPadding: '7 7 0',
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
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DOCNO_D'
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
            fieldLabel: '申請日期',
            name: 'APPTIME_T'
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
            fieldLabel: '軍民別',
            name: 'ISARMY',
            store: st_isarmy,
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
        }, {
            xtype: 'textfield',
            fieldLabel: '申請人姓名',
            name: 'APPUNA',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE',
            enforceMaxLength: true,
            maxLength: 100,
            //height: 200,
            readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            if (T1Form.getForm().findField('ISARMY').getValue() == '1') { //選擇軍款要提醒2次
                                Ext.MessageBox.confirm(confirmSubmit, '選擇軍款，是否確定?', function (btn, text) {
                                    if (btn === 'yes') {
                                        Ext.MessageBox.confirm(confirmSubmit, '選擇軍款，是否確定?', function (btn, text) {
                                            if (btn === 'yes') {
                                                T1Submit();
                                            }
                                        });
                                    }
                                });
                            } else {
                                T1Submit();
                            }
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
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
                            msglabel('訊息區:資料提出申請成功');
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
        TATabs.setActiveTab('Query');
        T1QueryForm.getForm().findField('P2').setValue(arrP2);
    }

    function showReport(docno) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?docno=' + docno + '&Order=mmcode" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        queryUrl: '/api/AB0127/GetMMCodeDocd', //指定查詢的Controller路徑
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
            labelWidth: 70
        },
        border: false,
        items: [
            T2QueryMMCode,
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T2Load(true);
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }
        ]
    });


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'FRWH_D', type: 'string' },
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
            { name: 'APP_AMT', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'SRCDOCNO', type: 'string' },
            { name: 'S_INV_QTY', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_T', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' }
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
            url: '/api/AB0127/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
            }
        }
    });
    function T2Load(loadFirst) {
        try {
            if (loadFirst) {
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
        matchFieldWidth: false,
        listConfig: { width: 190 },
        margin: '0 0 10 0',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0127/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'AVG_PRICE', 'TOT_APVQTY', 'PFILE_ID', 'HIGH_QTY', 'S_INV_QTY', 'INV_QTY', 'APPQTY_TIMES', 'UNITRATE', 'M_CONTID', 'M_AGENNO', 'SAFE_QTY'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: '01',
                p2: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                T2Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T2Form.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                T2Form.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                T2Form.getForm().findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                T2Form.getForm().findField('AVG_PRICE').setValue(r.get('AVG_PRICE'));
                T2Form.getForm().findField('PFILE_ID').setValue(r.get('PFILE_ID'));

                //選擇院內碼後顯示藥局按鈕(檢查庫存與消耗查詢)
                Ext.getCmp('btnGetInvqty').setDisabled(false);
                Ext.getCmp('btnGetUseqty').setDisabled(false);
                T2F1 = r.get('MMCODE'); //寫入檢查庫存對應項目變數
                T1F4 = T1Form.getForm().findField('TOWH').getValue(); //寫入消耗查詢對應項目變數

                if (r.get('WHMM_VALID') == 'Y') {
                    Ext.getCmp('WHMM_VALID').hide();
                } else {
                    Ext.getCmp('WHMM_VALID').show();
                }

                if (T2Form.getForm().findField('APPQTY').getValue()) {
                    // 申請金額 = 庫存平均單價 * 申請數量
                    var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                }
                T2Form.getForm().findField('HIGH_QTY').setValue(r.get('HIGH_QTY'));
                T2Form.getForm().findField('S_INV_QTY').setValue(r.get('S_INV_QTY'));
                T2Form.getForm().findField('INV_QTY').setValue(r.get('INV_QTY'));
                T2Form.getForm().findField('APPQTY_TIMES').setValue(r.get('APPQTY_TIMES'));
                T2Form.getForm().findField('UNITRATE').setValue(r.get('UNITRATE'));
                T2Form.getForm().findField('M_CONTID').setValue(r.get('M_CONTID'));
                T2Form.getForm().findField('M_AGENNO').setValue(r.get('M_AGENNO'));
                T2Form.getForm().findField('SAFE_QTY').setValue(r.get('SAFE_QTY'));
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
        bodyPadding: '7 7 0',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 110,
            msgTarget: 'side'
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
            name: 'SRCDOCNO',
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
                            popMmcodeForm_14(viewport, '/api/AB0127/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: '01',
                                WH_NO: T1Form.getForm().findField('TOWH').getValue(),
                                IS_MMCODEQ: 'Y',
                                Q_DRUGSNAME: 'Y'
                            }, setMmcode);
                        }
                    }
                }
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
            fieldLabel: '合約識別碼',
            name: 'M_CONTID'
        }, {
            xtype: 'displayfield',
            fieldLabel: '供應廠商',
            name: 'M_AGENNO'
        }, {
            xtype: 'displayfield',
            fieldLabel: '單價',
            name: 'M_CONTPRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '優惠最小單價',
            name: 'DISC_UPRICE',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '平均單價',
            name: 'AVG_PRICE',
            submitValue: true,
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請金額',
            name: 'APP_AMT',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請倍數',
            name: 'APPQTY_TIMES'
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
                    var parDISC_UPRICE = parseFloat(T2Form.getForm().findField('DISC_UPRICE').getValue());
                    var parAPPQTY = parseFloat(T2Form.getForm().findField('APPQTY').getValue());
                    T2Form.getForm().findField('APP_AMT').setValue(accMul(parDISC_UPRICE, parAPPQTY));
                }
            }
        },
        {
            xtype: 'label',
            text: '申請量必須符合申請倍數',
            style: 'color: #ff0000;',
            margin: '0 0 10 0'
        },
        {
            xtype: 'container',
            id: 'checkInvqty',
            hidden: true,
            minHeight: 40,
            layout: "hbox",
            align: 'stretch',
            items: [{
                xtype: 'button',
                itemId: 'btnGetInvqty',
                id: 'btnGetInvqty',
                text: '檢查庫存',
                disabled: true,
                margin: '0 0 0 50',
                padding: '2 0 5 0',
                handler: function () {
                    msglabel('訊息區:');
                    showWin8();
                }
            },
            {
                xtype: 'button',
                itemId: 'btnGetUseqty',
                id: 'btnGetUseqty',
                text: '消耗查詢',
                disabled: true,
                margin: '0 0 0 15',
                padding: '2 0 5 0',
                handler: function () {
                    msglabel('訊息區:');
                    showWin9();
                }
            }]
        },
        {
            xtype: 'displayfield',
            fieldLabel: '庫房存量',
            name: 'INV_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '基準量',
            name: 'HIGH_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '上級庫庫房存量',
            name: 'S_INV_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '出貨單位',
            name: 'UNITRATE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫房安全量',
            name: 'SAFE_QTY',
            submitValue: true
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
            triggerAction: 'all',
            xtype: 'hidden'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APLYITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 70,
            height: 200,
            readOnly: true
        }, {
            xtype: 'imagegrid',
            fieldLabel: '附加圖片',
            name: 'PFILE_ID',
            width: 300,
            height: 200,
            hidden: true
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
                    //var tot_apvqty = 0;
                    //if (this.up('form').getForm().findField('TOT_APVQTY').getValue() != null) {
                    //    tot_apvqty = Number(this.up('form').getForm().findField('TOT_APVQTY').getValue());
                    //}
                    var appqty = 0;
                    if (this.up('form').getForm().findField('APPQTY').getValue() != null) {
                        appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
                    }
                    var appqty_times = 0;
                    if (this.up('form').getForm().findField('APPQTY_TIMES').getValue() != null) {
                        appqty_times = Number(this.up('form').getForm().findField('APPQTY_TIMES').getValue());
                    }
                    var unitrate = 0;
                    if (this.up('form').getForm().findField('UNITRATE').getValue() != null) {
                        unitrate = Number(this.up('form').getForm().findField('UNITRATE').getValue());
                    }
                    if ((appqty % appqty_times) != 0) {
                        Ext.Msg.alert('提醒', '申請量必須符合申請倍數!');
                        isSub = false;
                    }
                    //var invqty = Number(this.up('form').getForm().findField('INV_QTY').getValue())
                    if (appqty > highqty) {
                        Ext.Msg.alert('提醒', '申請量不可超過基準量(庫房設定的單位請領量)');
                        isSub = false;
                    }
                }
                if (isSub) {
                    //20230913增加 桃園需求:檢查出貨單位但不卡死
                    if ((appqty % unitrate) != 0) {
                        Ext.MessageBox.confirm('提醒', '申請量不為出貨單位的倍數，是否仍要儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    } else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    }
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
                    if (f2.findField("x").getValue() == "I") {
                        T11Set = '../../../api/AB0127/CreateD';
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
                    T11Set = '../../../api/AB0127/CreateD';
                    Ext.getCmp('WHMM_VALID').hide();
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '../../../api/AB0127/UpdateD';
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
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('SEQ') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0127/DeleteD',
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
                                            T2Load(true);
                                        } else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
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
                itemId: 'getpk', text: '套餐轉入', disabled: true, hidden: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin4();
                }
            },
            {
                itemId: 'getsave', text: '低於安全存量轉入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    //viewport.down('#form').collapse();
                    showWin7();
                }
            },
            {
                itemId: 'getexport', text: '匯入', disabled: true, handler: function () {
                    msglabel('訊息區:');
                    showWin6();
                }
            },
            {
                itemId: 'detailExport', id: 'detailExport', text: '匯出', disabled: true, handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1LastRec.data.DOCNO });
                    p.push({ name: 'p1', value: T2Query.getForm().findField('P1').getValue() });
                    PostForm(detailExport, p);
                    msglabel('訊息區:匯出完成');
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
            Ext.getCmp('btnGetInvqty').setDisabled(true);
            Ext.getCmp('btnGetUseqty').setDisabled(true);//控制檢查庫存按鈕
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            f2.findField('SRCDOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);

            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('GTAPL_RESON').setReadOnly(false);
        f2.findField('APLYITEM_NOTE').setReadOnly(false);
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
            width: 170,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 70,
            sortable: true
        }, {
            text: "上級庫庫房存量",
            dataIndex: 'S_INV_QTY',
            width: 120,
            sortable: true
        }, {
            text: '撥發狀態',
            dataIndex: 'POSTID',
            width: 120
        }, {
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            width: 90,
            sortable: true
        }, {
            text: "庫房安全量",
            dataIndex: 'SAFE_QTY',
            width: 90,
            sortable: true
        }, {
            text: "單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 100, align: 'right',
            xtype: 'hidden',
        }, {
            text: "申請金額",
            dataIndex: 'APP_AMT',
            style: 'text-align:left',
            width: 140, align: 'right',
            xtype: 'hidden',
            renderer: function (val, meta, record) {
                // 申請金額 = 庫存平均單價 * 申請數量
                var parDISC_UPRICE = parseFloat(record.data['DISC_UPRICE']);
                var parAPPQTY = parseFloat(record.data['APPQTY']);
                return accMul(parDISC_UPRICE, parAPPQTY);
            }
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "核撥量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "送核撥時間",
            dataIndex: 'APL_CONTIME',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_D',
            width: 70
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID_T',
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

                if (T2Rec > 0) {
                    T2F1 = T2LastRec.get('MMCODE');
                }

                if (T2Grid.getSelection().length > 1) {
                    T2Grid.down('#edit').setDisabled(true);
                    Ext.getCmp('btnGetInvqty').setDisabled(true);
                    Ext.getCmp('btnGetUseqty').setDisabled(true);//控制檢查庫存按鈕
                }
                else
                    setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        T2Grid.down('#edit').setDisabled(T2Rec === 0);
        T2Grid.down('#delete').setDisabled(T2Rec === 0);
        Ext.getCmp('btnGetInvqty').setDisabled(T2Rec === 0);
        Ext.getCmp('btnGetUseqty').setDisabled(T2Rec === 0);//控制檢查庫存按鈕

        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            //f.findField('DATA_SEQ_O').setValue(T2LastRec.get('DATA_SEQ'));
            //var u = f.findField('ID');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            if (T1F3 === '0101' || T1F3 === '0601') {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
                Ext.getCmp('btnGetInvqty').setDisabled(false);
                Ext.getCmp('btnGetUseqty').setDisabled(false);//控制檢查庫存按鈕
            }
            else {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
                Ext.getCmp('btnGetInvqty').setDisabled(true);
                Ext.getCmp('btnGetUseqty').setDisabled(true);//控制檢查庫存按鈕
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
        bodyPadding: '7 7 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
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
                                url: '/api/AB0127/Savepk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1F1,
                                    MAT_CLASS: '01',
                                    DOCTYPE: T1F9,
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
        }]
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
            { name: 'APPQTY_TIMES', type: 'string' }
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
            url: '/api/AB0127/GetPackD',
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
                    p2: '01',
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
            labelWidth: 90
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
            width: '700px',
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
                        if (selection[i].data.APPQTY % selection[i].data.APPQTY_TIMES != 0) {
                            if (msg != '') {
                                msg += '<br>';
                            }
                            msg += ('院內碼:' + selection[i].data.MMCODE + ' 申請量:' + selection[i].data.APPQTY + ' 申請倍數:' + selection[i].data.APPQTY_TIMES);
                        }
                    }
                    if (msg != '') {
                        msg = '<span style="color:red">申請量須符合申請倍數，請至套餐建置修改</span><br>' + msg;
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
                                url: '/api/AB0127/InsFromPk',
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
                                        hideWin4();
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
            width: 170,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 70,
            sortable: true
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "申請倍數",
            dataIndex: 'APPQTY_TIMES',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            header: "",
            flex: 1
        }
        ]
    });

    var T7Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0127/GetSaveD',
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
                    p2: '01',
                    p3: T1F4
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T7Load() {
        T7Store.load({
            params: {
                start: 0
            }
        });
    }
    var T7Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 90
        },
        border: false,
        items: [{
            xtype: 'button', text: '轉入',
            id: 'T7btn2',
            handler: function () {
                var selection = T7Grid.getSelection();
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
                            var myMask = new Ext.LoadMask(win7, { msg: '處理中...' });
                            myMask.show();
                            Ext.Ajax.request({
                                url: '/api/AB0127/InsFromPk',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    MMCODE: mmcode,
                                    APPQTY: appqty
                                },
                                timeout: 0,
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.MessageBox.alert('訊息', '轉入成功');
                                        msglabel('訊息區:轉入成功');
                                        hideWin7();
                                        //T2Store.removeAll();
                                        T2Grid.getSelectionModel().deselectAll();
                                        T2Load(true);
                                    }
                                    else {
                                        myMask.hide();
                                        hideWin7();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    myMask.hide();
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    hideWin7();
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

    function T7Cleanup() {
        T7Query.getForm().reset();
        T7Load();
        msglabel('訊息區:');
    }
    var T7Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T7Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T7Query]
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
            width: 170,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 70,
            sortable: true
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            width: 90, align: 'right'
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 90, align: 'right'
        //}, {
        //    text: "超量原因",
        //    dataIndex: 'GTAPL_RESON_N',
        //    style: 'text-align:left',
        //    width: 170, align: 'right'
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
            //url: '/api/AB0127/GetSaveD',
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
                    p2: '01',
                    p3: T1F7
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
            labelWidth: 90
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 't6export',
                name: 'T6insert',
                text: '範本', handler: function () {
                    var p = new Array();
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
                            formData.append("matclass", '01');
                            formData.append("docno", T1F1);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AB0127/SendExcel",
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
                        url: '/api/AB0127/Insert',
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
            dataIndex: 'CHECK_RESULT',
            width: 200,
            hidden: true,
            renderer: function (val, meta, record) {
                if (record.data['APPQTY'] == '0')
                    return '<font color=blue>' + record.data['CHECK_RESULT'] + '</font>'; // 申請數量=0以藍字顯示
                else
                    return record.data['CHECK_RESULT'];
            }
        },
        {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 100,
            sortable: true,
            renderer: function (val, meta, record) {
                if (record.data['APPQTY'] == '0')
                    return '<font color=blue>' + record.data['DOCNO'] + '</font>'; // 申請數量=0以藍字顯示
                else
                    return record.data['DOCNO'];
            }
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true,
            renderer: function (val, meta, record) {
                if (record.data['APPQTY'] == '0')
                    return '<font color=blue>' + record.data['MMCODE'] + '</font>'; // 申請數量=0以藍字顯示
                else
                    return record.data['MMCODE'];
            }
        },
        {
            text: "申請數量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 90, align: 'right',
            renderer: function (val, meta, record) {
                if (record.data['APPQTY'] == '0')
                    return '<font color=blue>' + record.data['APPQTY'] + '</font>'; // 申請數量=0以藍字顯示
                else
                    return record.data['APPQTY'];
            }
        },
        {
            text: "備註",
            dataIndex: 'APLYITEM_NOTE',
            style: 'text-align:left',
            width: 90,
            renderer: function (val, meta, record) {
                if (record.data['APPQTY'] == '0')
                    return '<font color=blue>' + record.data['APLYITEM_NOTE'] + '</font>'; // 申請數量=0以藍字顯示
                else
                    return record.data['APLYITEM_NOTE'];
            }
        },
        {
            header: "",
            flex: 1
        }
        ]
    });

    Ext.define('T8Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'NORMAL_QTY', type: 'string' }
        ]
    });
    var T8Store = Ext.create('Ext.data.Store', {
        model: 'T8Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0127/GetInvqty',
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
                    p0: T2F1
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })

    function T8Load() {
        T8Store.load({
            params: {
                start: 0
            }
        });
    }

    function T8Cleanup() {
        T8Load();
        msglabel('訊息區:');
    }


    var T8Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T8Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        columns: [
            { xtype: 'rownumberer' },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100,
                sortable: true
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 120,
                sortable: true
            }, {
                text: "現存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                text: "安全存量",
                dataIndex: 'SAFE_QTY',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                text: "基準量",
                dataIndex: 'NORMAL_QTY',
                style: 'text-align:left',
                width: 100, align: 'right'
            }, {
                header: "",
                flex: 1
            }
        ]
    });

    var T9Store = Ext.create('WEBAPP.store.AB0129', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F4,
                    p1: T2F1
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T9Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T9LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T9Form.getForm().reset();
                    }
                }
                T9Form.loadRecord(T9Store.data.items[0]);
            }
        }
    });

    function T9Load() {
        T9Store.load({
            params: {
                start: 0
            }
        });
    }

    function T9Cleanup() {
        T9Load();
        msglabel('訊息區:');
    }

    var mLabelWidth = 70;
    var mWidth = 230;
    var T9Form = Ext.create('Ext.form.Panel', {
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
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '90%',
                flex: 1,
                minWidth: 70,
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
                        //items: [TATabs]
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
                padding: 7,
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

    var win7;
    if (!win7) {
        win7 = Ext.widget('window', {
            title: '低於安全存量',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T7Grid],
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

    function showWin7() {
        if (win7.hidden) {
            T7Cleanup();
            T7Load();
            win7.show();
        }
    }
    function hideWin7() {
        if (!win7.hidden) {
            win7.hide();
            T7Cleanup();
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

    var win8;
    if (!win8) {
        win8 = Ext.widget('window', {
            title: '檢查庫存',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T8Grid],
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

    function showWin8() {
        if (win8.hidden) {
            T8Cleanup();
            T8Load();
            win8.show();
        }
    }
    function hideWin8() {
        if (!win8.hidden) {
            win8.hide();
            T8Cleanup();
        }
    }

    var win9;
    if (!win9) {
        win9 = Ext.widget('window', {
            title: '消耗查詢',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T9Form],
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

    function showWin9() {
        if (win9.hidden) {
            T9Cleanup();
            T9Load();
            win9.show();
        }
    }
    function hideWin9() {
        if (!win9.hidden) {
            win9.hide();
            T9Cleanup();
        }
    }


    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P2').setValue(arrP2);

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
});
