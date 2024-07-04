
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0007/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "能設申請核撥";
    var T3GetExcel = '/api/AB0007/Excel3';
    var T4GetExcel = '/api/AB0007/Excel4';

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP2 = ["1", "2"]; 
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0007/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0007/GetFlowidCombo',
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
            url: '/api/AB0007/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_jcn = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0007/GetJcnCombo',
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
            url: '/api/AB0007/GetTowhCombo',
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
            url: '/api/AB0007/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
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
                fieldLabel: '物料分類',
                name: 'P3',
                id: 'P3',
                store: st_matclass,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '工單號碼',
                name: 'P4',
                id: 'P4',
                store: st_jcn,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                name: 'P5',
                id: 'P5',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                T1Load();
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
            { name: 'JCN', type: 'string' }        ]
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
            url: '/api/AB0007/AllM',
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
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p4: T1QueryForm.getForm().findField('P4').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue()
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
                    T1Set = '/api/AB0007/CreateM';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0007/UpdateM';
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
                                    url: '/api/AB0007/DeleteM',
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
                itemId: 'apply', text: '核撥', disabled: true, handler: function () {
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
                        Ext.MessageBox.confirm('核撥', '是否確定核撥？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0007/Apply',
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
                                            msglabel('訊息區:核撥成功');
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
                itemId: 'applyr', text: '取消核撥', disabled: true, handler: function () {
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
                        Ext.MessageBox.confirm('取消核撥', '是否確定取消核撥？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0007/ApplyR',
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
                                            msglabel('訊息區:取消核撥成功');
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
                itemId: 'export1', text: '單位消耗品項明細檔下載', handler: function () {
                    showWin3();
                }
            }, {
                itemId: 'export2', text: '品項總消耗檔下載', handler: function () {
                    showWin4();
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
            f.findField('FRWH_N').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('APPDEPT_NAME').setValue(st_getlogininfo.getAt(0).get('INIDNAME'));
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('EF1');
            f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));
            f.findField('MAT_CLASS').setReadOnly(false);
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('TOWH').setReadOnly(false);
        f.findField('JCN').setReadOnly(false);
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
            width: 150
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
            text: "工單號碼",
            dataIndex: 'JCN',
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
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#applyr').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
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
            if (T1F3 == '1') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#apply').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                T1Grid.down('#apply').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
            }
            if (T1F3 == '2') {

                T1Grid.down('#applyr').setDisabled(false);
            }
            else {

                T1Grid.down('#applyr').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
        T2Query.getForm().reset();
        T2Load();
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
            fieldLabel: '工單號碼',
            name: 'JCN',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
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

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0007/GetMMCodeDocd', //指定查詢的Controller路徑
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
            T2QueryMMCode,
            {
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
            }]
    });


    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'M_APPQTY', type: 'string' },
            { name: 'S_APPQTY', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'APVTIME', type: 'string' },
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
            { name: 'M_AGENNO', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'T_APPQTY', type: 'string' },
            { name: 'EXCH_RATIO', type: 'string' },
            { name: 'A_PACK', type: 'string' },
            { name: 'M_PACK', type: 'string' },
            { name: 'S_PACK', type: 'string' },
            { name: 'T_PACK', type: 'string' },
            { name: 'MIN_PRICE', type: 'string' },
            { name: 'T_PRICE', type: 'string' },
            { name: 'A_INV_QTY', type: 'string' },
            { name: 'M_INV_QTY', type: 'string' },
            { name: 'S_INV_QTY', type: 'string' },
            { name: 'A_APV_QTY', type: 'string' },
            { name: 'M_APV_QTY', type: 'string' },
            { name: 'S_APV_QTY', type: 'string' },
            { name: 'TOT_APPQTY', type: 'string' },
            { name: 'TOT_APVQTY', type: 'string' }
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
            url: '/api/AB0007/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
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
        queryUrl: '/api/AB0007/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'M_CONTPRICE', 'M_AGENNO', 'AGEN_NAMEC', 'M_PURUN', 'EXCH_RATIO', 'A_INV_QTY', 'M_INV_QTY', 'S_INV_QTY', 'EXCH_RATIO'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('TOWH').getValue()
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
                T2Form.getForm().findField('A_INV_QTY').setValue(r.get('A_INV_QTY'));
                T2Form.getForm().findField('M_INV_QTY').setValue(r.get('M_INV_QTY'));
                T2Form.getForm().findField('S_INV_QTY').setValue(r.get('S_INV_QTY'));
                T2Form.getForm().findField('EXCH_RATIO').setValue(r.get('EXCH_RATIO'));
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
                            
                            popMmcodeFormEC(viewport, '/api/AB0007/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                                WH_NO: T1Form.getForm().findField('TOWH').getValue()
                            }, setMmcode);
                        }
                    }
                },

            ]
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
            fieldLabel: '醫院民庫存量',
            name: 'A_INV_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '醫院軍庫存量',
            name: 'M_INV_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '學院軍庫存量',
            name: 'S_INV_QTY'
        }, {
            fieldLabel: '醫院民申請量',
            name: 'APPQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                change: function () {
                    var objP0 = T2Form.getForm().findField("APPQTY");
                    var objP1 = T2Form.getForm().findField("M_APPQTY");
                    var objP2 = T2Form.getForm().findField("S_APPQTY");
                    var objP5 = T2Form.getForm().findField("T_APPQTY");
                    if (objP0.getValue() != '' && objP1.getValue() != '' && objP2.getValue() != '') {
                        objP5.setValue(parseFloat(objP0.getValue()) + parseFloat(objP1.getValue()) + parseFloat(objP2.getValue()));
                    }
                }
            }
        }, {
            fieldLabel: '醫院軍申請量',
            name: 'M_APPQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                change: function () {
                    var objP0 = T2Form.getForm().findField("APPQTY");
                    var objP1 = T2Form.getForm().findField("M_APPQTY");
                    var objP2 = T2Form.getForm().findField("S_APPQTY");
                    var objP5 = T2Form.getForm().findField("T_APPQTY");
                    if (objP0.getValue() != '' && objP1.getValue() != '' && objP2.getValue() != '') {
                        objP5.setValue(parseFloat(objP0.getValue()) + parseFloat(objP1.getValue()) + parseFloat(objP2.getValue()));
                    }
                }
            }
        }, {
            fieldLabel: '學院軍申請量',
            name: 'S_APPQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                change: function () {
                    var objP0 = T2Form.getForm().findField("APPQTY");
                    var objP1 = T2Form.getForm().findField("M_APPQTY");
                    var objP2 = T2Form.getForm().findField("S_APPQTY");
                    var objP5 = T2Form.getForm().findField("T_APPQTY");
                    if (objP0.getValue() != '' && objP1.getValue() != '' && objP2.getValue() != '') {
                        objP5.setValue(parseFloat(objP0.getValue()) + parseFloat(objP1.getValue()) + parseFloat(objP2.getValue()));
                    }
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '總申請量',
            name: 'T_APPQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '最小撥補量',
            name: 'EXCH_RATIO'
        }, {
            xtype: 'label',
            text: '申請量必須為最小撥補量的倍數',
            style: 'color: #ff0000;'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'APLYITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 50,
            height: 200,
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                var isSub = true;
                if (this.up('form').getForm().findField('APPQTY').getValue() == '0' && this.up('form').getForm().findField('M_APPQTY').getValue() == '0' && this.up('form').getForm().findField('S_APPQTY').getValue() == '0'){
                    Ext.Msg.alert('提醒', '申請數量不可為0');
                    isSub = false;
                }
                else {
                    var appqty = 0;
                    if (this.up('form').getForm().findField('APPQTY').getValue() != null) {
                        appqty = Number(this.up('form').getForm().findField('APPQTY').getValue());
                    }
                    var m_appqty = 0;
                    if (this.up('form').getForm().findField('M_APPQTY').getValue() != null) {
                        m_appqty = Number(this.up('form').getForm().findField('M_APPQTY').getValue());
                    }
                    var s_appqty = 0;
                    if (this.up('form').getForm().findField('S_APPQTY').getValue() != null) {
                        s_appqty = Number(this.up('form').getForm().findField('S_APPQTY').getValue());
                    }
                    var minodrqty = 0;
                    if (this.up('form').getForm().findField('EXCH_RATIO').getValue() != null) {
                        minodrqty = Number(this.up('form').getForm().findField('EXCH_RATIO').getValue());
                    }
                    if (((appqty % minodrqty) != 0)||((m_appqty % minodrqty) != 0) || ((s_appqty % minodrqty) != 0) ) {
                        Ext.Msg.alert('提醒', '申請量必須為最小撥補量的倍數!');
                        isSub = false;
                    }
                    if (isSub) {
                        var strsubmit = false;
                        var v_appqty = parseInt(T2Form.getForm().findField("APPQTY").getValue(), 10);
                        var v_mappqty = parseInt(T2Form.getForm().findField("M_APPQTY").getValue(), 10);
                        var v_sappqty = parseInt(T2Form.getForm().findField("S_APPQTY").getValue(), 10);
                        var v_a_invqty = parseInt(T2Form.getForm().findField("A_INV_QTY").getValue(), 10);
                        var v_m_invqty = parseInt(T2Form.getForm().findField("M_INV_QTY").getValue(), 10);
                        var v_s_invqty = parseInt(T2Form.getForm().findField("S_INV_QTY").getValue(), 10);
                        if (T1F4.substring(0, 2) == "55" || T1F4.substring(1, 1) == "M") {
                            strsubmit = true;
                        }
                        else {
                            if (T1F4.substring(0, 1) == "A" && T1F4.substring(1, 1) != "M") {
                                if (v_appqty != 0 || v_mappqty != 0) {
                                    strsubmit = false;
                                    Ext.Msg.alert('提醒', '您只能申請學院軍');
                                }
                                else {
                                    strsubmit = true;
                                }
                            }
                            else {
                                if (v_sappqty != 0) {
                                    strsubmit = false;
                                    Ext.Msg.alert('提醒', '您只能申請醫院民、醫院軍');
                                }
                                else {
                                    strsubmit = true;
                                }
                            }
                        }
                        if (v_appqty > v_a_invqty || v_mappqty > v_m_invqty || v_sappqty > v_s_invqty) {
                            strsubmit = false;
                            Ext.Msg.alert('提醒', '申請量超過庫存量');
                        }
                        if (strsubmit) {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T2Submit();
                                }
                            });
                        }
                    }
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
                        T11Set = '../../../api/AB0007/CreateD';
                        setFormT2('I', '新增');
                    }
                    else {
                        T2Cleanup();
                        T2Load();
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
        f.findField('M_APPQTY').setReadOnly(true);
        f.findField('S_APPQTY').setReadOnly(true);
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
                    T11Set = '../../../api/AB0007/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '../../../api/AB0007/UpdateD';
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
                                    url: '/api/AB0007/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
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
            f2.findField('APPQTY').setValue('0');
            f2.findField('M_APPQTY').setValue('0');
            f2.findField('S_APPQTY').setValue('0');
            f2.findField('T_APPQTY').setValue('0');
            T2Form.down('#btnMmcode').setVisible(true);

            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APPQTY');
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('APPQTY').setReadOnly(false);
        f2.findField('M_APPQTY').setReadOnly(false);
        f2.findField('S_APPQTY').setReadOnly(false);

        if (T1F4.substring(0, 2) == "55" || T1F4.substring(1, 1) == "M") {
            f2.findField('APPQTY').setReadOnly(false);
            f2.findField('M_APPQTY').setReadOnly(false);
            f2.findField('S_APPQTY').setReadOnly(false);
        }
        else {
            if (T1F4.substring(0, 1) == "A" && T1F4.substring(1, 1) != "M") {
                f2.findField('APPQTY').setReadOnly(true);
                f2.findField('M_APPQTY').setReadOnly(true);
            }
            else {
                f2.findField('S_APPQTY').setReadOnly(true);
            }
        }

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
            width: 150,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "進貨單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "醫院民庫存量",
            dataIndex: 'A_INV_QTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "醫院軍庫存量",
            dataIndex: 'M_INV_QTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "學院軍庫存量",
            dataIndex: 'S_INV_QTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "醫院民申請量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "醫院軍申請量",
            dataIndex: 'M_APPQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "學院軍申請量",
            dataIndex: 'S_APPQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "總申請量",
            dataIndex: 'TOT_APPQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "醫院民累計核撥",
            dataIndex: 'A_APV_QTY',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
                text: "醫院軍累計核撥",
            dataIndex: 'M_APV_QTY',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
                text: "學院軍累計核撥",
            dataIndex: 'S_APV_QTY',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
                text: "累計總核撥",
            dataIndex: 'TOT_APVQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
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
        autoScroll: true,
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '起始日期',
                name: 'T3D0',
                id: 'T3D0',
                vtype: 'dateRange',
                dateRange: { end: 'T3D1' },
                padding: '0 4 0 4', value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '截止日期',
                name: 'T3D1',
                id: 'T3D1',
                vtype: 'dateRange',
                dateRange: { begin: 'T3D0' },
                padding: '0 4 0 4', value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T3Submit', text: '確定', handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '單位消耗品項明細檔.xls' });
                p.push({ name: 'd0', value: T3Form.getForm().findField('T3D0').rawValue });
                p.push({ name: 'd1', value: T3Form.getForm().findField('T3D1').rawValue });
                PostForm(T3GetExcel, p);
                msglabel('訊息區:匯出完成');
                hideWin3();
            }
        },
        {
            itemId: 'cancel', text: '離開', handler: hideWin3
        }
        ]
    });
    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '單位消耗品項明細檔',
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
        }
    }

    var T4Form = Ext.widget({
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
        autoScroll: true,
        items: [
            {
                xtype: 'monthfield',
                fieldLabel: '起始年月',
                name: 'T4D0',
                id: 'T4D0',
                padding: '0 4 0 4',
                value: new Date()
            }, {
                xtype: 'monthfield',
                fieldLabel: '截止年月',
                name: 'T4D1',
                id: 'T4D1',
                padding: '0 4 0 4',
                value: new Date()
            }
        ],
        buttons: [{
            itemId: 'T4Submit', text: '確定', handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '品項總消耗檔.xls' });
                p.push({ name: 'd0', value: T4Form.getForm().findField('T4D0').rawValue });
                p.push({ name: 'd1', value: T4Form.getForm().findField('T4D1').rawValue });
                PostForm(T4GetExcel, p);
                msglabel('訊息區:匯出完成');
                hideWin4();
            }
        },
        {
            itemId: 'cancel', text: '離開', handler: hideWin4
        }
        ]
    });
    var win4;
    var winActWidth4 = 300;
    var winActHeight4 = 200;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '品項總消耗檔',
            closeAction: 'hide',
            width: winActWidth4,
            height: winActHeight4,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T4Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth4 > 0) ? winActWidth4 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight4 > 0) ? winActHeight4 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth4 = width;
                    winActHeight4 = height;
                }
            }
        });
    }

    function showWin4() {
        if (win4.hidden) {
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
        }
    }
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
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });
    
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P2').setValue(arrP2);
});
