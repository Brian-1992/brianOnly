
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0025/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "藥品換貨作業";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var reportUrl = '/Report/A/AA0025.aspx';
    var reportBUrl = '/Report/A/AA0025B.aspx';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_agen = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetAgenCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_STKTRANSKIND = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetSTKTRANSKINDCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_lotno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetLotNoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T2Form.getForm().findField('MMCODE').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var st_stat = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetStatCombo',
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
            url: '/api/AA0025/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0025/GetFlowidCombo',
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
    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function addMonths(date, months) {
        date.setMonth(date.getMonth() + months);
        return date;
    }
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70
        },
        border: false,
        items: [
            {
                xtype: 'combo',
                fieldLabel: '換貨單號',
                labelAlign: 'right',
                name: 'PP',
                id: 'PP',
                store: st_docno,
                queryMode: 'local',
                labelWidth: 60,
                width: 180,
                displayField: 'TEXT',
                valueField: 'VALUE'
            },{
                xtype: 'datefield',
                fieldLabel: '換貨日期區間',
                labelAlign: 'right',
                name: 'P0',
                id: 'P0',
                vtype: 'dateRange',
                dateRange: { end: 'P1' },
                padding: '0 4 0 4',
                labelWidth: 80,
                width: 170,
                value: get6monthday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: 10,
                name: 'P1',
                id: 'P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'P0' },
                padding: '0 4 0 4',
                width: 90,
                value: getToday()
            }, {
                xtype: 'hidden',
                fieldLabel: '換貨廠商',
                labelAlign: 'right',
                name: 'P2',
                id: 'P2'
            }, {
                xtype: 'combo',
                fieldLabel: '換貨類別',
                labelAlign: 'right',
                name: 'P3',
                id: 'P3',
                store: st_STKTRANSKIND,
                queryMode: 'local',
                multiSelect: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                displayField: 'TEXT',
                valueField: 'VALUE',
                labelWidth: 80,
                width: 180
            }, {
                xtype: 'combo',
                fieldLabel: '狀態',
                labelAlign: 'right',
                name: 'P4',
                id: 'P4',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'TEXT',
                valueField: 'VALUE',
                labelWidth: 40,
                width: 130
            },
            {
                xtype: 'button',
                text: '查詢',
                handler: T1Load
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
            { name: 'STKTRANSKIND', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
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
            { name: 'STKTRANSKIND_N', type: 'string' },
            { name: 'AGEN_NO_N', type: 'string' },
            { name: 'SUM_EX', type: 'string' },
            { name: 'SUM_IN', type: 'string' }
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
            url: '/api/AA0025/AllM',
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
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    pp: T1Query.getForm().findField('PP').getValue()
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
                    T1Set = '/api/AA0025/CreateM';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0025/UpdateM';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '撤銷', disabled: true,
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
                        Ext.MessageBox.confirm('撤銷', '是否確定撤銷換貨單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0025/DeleteM',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.MessageBox.alert('訊息', '撤銷換貨單號<br>' + name + '成功');
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
                itemId: 'apply', text: '執行過帳', disabled: true, handler: function () {
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
                        Ext.MessageBox.confirm('執行過帳', '是否確定執行過帳？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0025/Apply',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '執行過帳成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:執行過帳成功');
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
                itemId: 'deleteall', text: '刪除', disabled: true,
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
                                    url: '/api/AA0025/DeleteAll',
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
                itemId: 'print', text: '列印',  hidde:true, handler: function () {
                    showWin3();
                }
            },
            {
                itemId: 'print2', text: '列印換貨清單', handler: function () {
                    showWin4();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
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
            f.findField('APPTIME_T').setValue(st_getlogininfo.getAt(0).get('TWN_SYSDATE'));
            f.findField('APP_NAME').setValue(st_getlogininfo.getAt(0).get('USERNAME'));
            f.findField('FLOWID').setValue('0801');
            f.findField('DOCTYPE').setValue('EX');
            f.findField('MAT_CLASS').setValue('01');
        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('STKTRANSKIND').setReadOnly(false);
        //.findField('AGEN_NO').setReadOnly(false);
        f.findField('APPTIME_T').setReadOnly(false);
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
            text: "換貨單號",
            dataIndex: 'DOCNO',
            width: 100
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 80
        }, {
            text: "換貨日期",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "換貨類別",
            dataIndex: 'STKTRANSKIND_N',
            width: 80
        }, {
            text: "備註",
            dataIndex: 'APPLY_NOTE',
            width: 180
        }, {
            text: "建立人員",
            dataIndex: 'APP_NAME',
            width: 80
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
        T1Grid.down('#deleteall').setDisabled(T1Rec === 0);
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        viewport.down('#form').expand();
        T2Query.getForm().findField('SUM_EX').setValue('');
        T2Query.getForm().findField('SUM_IN').setValue('');
        T2Query.getForm().findField('BALANCE').setValue('');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('DOCNO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            f.findField('MAT_CLASS').setReadOnly(true);
            T1F1 = f.findField('DOCNO').getValue();
            f.findField('DOCNO_D').setValue(T1F1);
            T1F2 = f.findField('MAT_CLASS').getValue();
            T1F3 = f.findField('FLOWID').getValue();
            T1F4 = f.findField('STKTRANSKIND').getValue();

            if (T1F3 == '0801' || T1F3 == 'X') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#apply').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#deleteall').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#apply').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                T1Grid.down('#deleteall').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
            }

        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
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
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'MAT_CLASS',
            xtype: 'hidden'
        }, {
            name: 'APPTIME',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '換貨單號',
            name: 'DOCNO_D'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'FLOWID_N'
        }, {
            xtype: 'datefield',
            fieldLabel: '換貨日期',
            name: 'APPTIME_T',
            readOnly: true
        }, {
            xtype: 'combo',
            fieldLabel: '換貨類別',
            name: 'STKTRANSKIND',
            store: st_STKTRANSKIND,
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
            xtype: 'displayfield',
            fieldLabel: '建立人員',
            name: 'APP_NAME'
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

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

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
                    f2.findField('APPTIME').setValue(f2.findField('APPTIME_T').getValue());
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('PP').setValue(v.DOCNO);
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
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var GetLOT_NO = '../../../api/AA0025/GetLOT_NO';
    var GetEXP_DATE = '../../../api/AA0025/GetEXP_DATE';
    var GetINV_QTY = '../../../api/AA0025/GetINV_QTY';
    var GetM_DISCPERC = '../../../api/AA0025/GetM_DISCPERC';
    var GetM_CONTPRICE = '../../../api/AA0025/GetM_CONTPRICE';

    var DISCPERC = '';
    var CONTPRICE = '';
    var NEW_APVQTY = '';

    var LOT_NO_Store = Ext.create('Ext.data.Store', {  //調帳庫別的store
        fields: ['LOT_NO', 'EXP_DATE', 'INV_QTY']
    });

    function SetLOT_NO(MMCODE) { //建立批號的下拉式選單
        var qty_temp = 0;
        Ext.Ajax.request({
            url: GetLOT_NO,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: MMCODE
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var lot_nos = data.etts;
                    LOT_NO_Store.removeAll();
                    if (lot_nos.length > 0) {
                        
                        LOT_NO_Store.add({ LOT_NO: '', EXP_DATE: '', INV_QTY: '' });
                        for (var i = 0; i < lot_nos.length; i++) {
                            LOT_NO_Store.add({ LOT_NO: lot_nos[i].LOT_NO, EXP_DATE: lot_nos[i].EXP_DATE, INV_QTY: lot_nos[i].INV_QTY });
                            T2Form.getForm().findField("LOT_NO").setValue(lot_nos[0].LOT_NO);
                            T2Form.getForm().findField("EXP_DATE").setValue(lot_nos[0].EXP_DATE);
                            T2Form.getForm().findField("EXP_DATE_T").setValue(lot_nos[0].EXP_DATE);
                            T2Form.getForm().findField("INV_QTY").setValue(lot_nos[0].INV_QTY);
                            qty_temp = lot_nos[0].INV_QTY;
                        }
                        T2Form.getForm().findField("APVQTY").setValue(T2Form.getForm().findField("INV_QTY").getValue());
                        if (T2Form.getForm().findField("APVQTY").getValue() == null) {
                            T2Form.getForm().findField("APVQTY").setValue(qty_temp);
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetEXP_DATE() { //取得效期
        Ext.Ajax.request({
            url: GetEXP_DATE,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue(),
                LOT_NO: T2Form.getForm().findField("LOT_NO").getValue()
            },
            success: function (response) {
                T2Form.getForm().findField("EXP_DATE").setValue(response.responseText);
            },
            failure: function (response, options) {
            }
        });
    }

    function SetINV_QTY() { //取得效期數量
        Ext.Ajax.request({
            url: GetINV_QTY,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue(),
                LOT_NO: T2Form.getForm().findField("LOT_NO").getValue()
            },
            success: function (response) {
                T2Form.getForm().findField("INV_QTY").setValue(response.responseText);
            },
            failure: function (response, options) {
            }
        });
    }

    function SetM_DISCPERC() { //取得進貨單價
        Ext.Ajax.request({
            url: GetM_DISCPERC,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                DISCPERC = response.responseText.replace(new RegExp('"', 'g'), '');
                if (DISCPERC == null) {
                    DISCPERC = 0;
                }
                Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ' + DISCPERC);
                NEW_APVQTY = T2Form.getForm().findField("APVQTY").getValue();
                if (T2Form.getForm().findField('IN_PRICE').getValue() != '' &&
                    T2Form.getForm().findField('IN_PRICE').getValue() != null) {
                    T2Form.getForm().findField("IN_PRICE_1").setValue(true);
                    T2Form.getForm().findField("CONTPRICE_1").setValue(false);
                    T2Form.getForm().findField("C_AMT").setValue(Math.round(NEW_APVQTY * DISCPERC * 100) / 100);
                }
                else if (T2Form.getForm().findField('CONTPRICE').getValue() != '' &&
                    T2Form.getForm().findField('CONTPRICE').getValue() != null) {
                    T2Form.getForm().findField("IN_PRICE_1").setValue(false);
                    T2Form.getForm().findField("CONTPRICE_1").setValue(true);
                    T2Form.getForm().findField("C_AMT").setValue(Math.round(NEW_APVQTY * CONTPRICE * 100) / 100);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetM_CONTPRICE() { //取得合約單價
        Ext.Ajax.request({
            url: GetM_CONTPRICE,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                CONTPRICE = response.responseText.replace(new RegExp('"', 'g'), '');
                Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + CONTPRICE);
            },
            failure: function (response, options) {
            }
        });
    }

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0025/GetMMCodeDocd', //指定查詢的Controller路徑
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
            labelWidth: 100
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
            }, {
                xtype: 'displayfield',
                fieldLabel: '換出總金額',
                labelAlign: 'right',
                name: 'SUM_EX',
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        return '<span style="color:red">' + Ext.util.Format.number(val, "0,000") + '</span>';
                    }
                }
            }, {
                xtype: 'displayfield',
                fieldLabel: '換入總金額',
                labelAlign: 'right',
                name: 'SUM_IN',
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        return '<span style="color:green">' + Ext.util.Format.number(val, "0,000") + '</span>';
                    }
                }
            }, {
                xtype: 'displayfield',
                fieldLabel: 'BALANCE',
                labelAlign: 'right',
                name: 'BALANCE',
                renderer: function (val, meta, record) {
                    if (val != undefined) {
                        var num = Number(val);
                        if (num >= 0) {
                            return '<span style="color:green">' + Ext.util.Format.number(val, "0,000") + '</span>';
                        }
                        else {
                            return '<span style="color:red">' + Ext.util.Format.number(val, "0,000") + '</span>';
                        }
                    }
                }
            }

        ]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'C_TYPE', type: 'string' },
            { name: 'C_STATUS', type: 'string' },
            { name: 'C_RNO', type: 'string' },
            { name: 'C_AMT', type: 'string' },
            { name: 'C_UP', type: 'string' },
            { name: 'ITEM_NOTE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'BALANCE', type: 'string' },
            { name: 'LOT_NO_N', type: 'string' },
            { name: 'EXP_DATE_T', type: 'string' },
            { name: 'SUM_EX', type: 'string' },
            { name: 'SUM_IN', type: 'string' }
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
            url: '/api/AA0025/AllD',
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
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    {
                        if (records.length > 0) {
                            T2Query.getForm().findField('SUM_EX').setValue(records[0].data.SUM_EX);
                            T2Query.getForm().findField('SUM_IN').setValue(records[0].data.SUM_IN);
                            T2Query.getForm().findField('BALANCE').setValue(records[0].data.BALANCE);
                        }
                    }
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
    function setLotno(args) {
        var f = T2Form.getForm();
        if (args.LOTNO !== '') {
            f.findField("LOT_NO").setValue(args.LOT_NO);
            f.findField("LOT_NO_N").setValue(args.LOT_NO);
            f.findField("EXP_DATE").setValue(args.EXP_DATE);
            f.findField("EXP_DATE_T").setValue(args.EXP_DATE);
            f.findField("INV_QTY").setValue(args.INV_QTY);
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
        queryUrl: '/api/AA0025/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'INV_QTY', 'M_CONTPRICE','M_DISCPERC'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('MAT_CLASS').getValue(),
                p2: T1Form.getForm().findField('DOCNO').getValue(),
                p3: T2Form.getForm().findField('INOUT').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                    f.findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                    f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEC'));
                    f.findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                    f.findField('INV_QTY').setValue(r.get('INV_QTY'));
                    f.findField('M_CONTPRICE').setValue(r.get('M_CONTPRICE'));
                    f.findField('IN_PRICE').setValue(r.get('M_DISCPERC'));
                    f.findField('CONTPRICE').setValue(r.get('M_CONTPRICE'));
                    Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ' + r.get('M_DISCPERC'));
                    Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + r.get('M_CONTPRICE'));

                    DISCPERC = r.get('M_DISCPERC');
                    CONTPRICE = r.get('M_CONTPRICE');
                    if (this.up('form').getForm().findField('INOUT').getValue() == '2') {
                        SetLOT_NO(r.get('MMCODE'));
                    }
                    //st_lotno.load();
                    //var f2 = T2Form.getForm();
                    //if (f2.findField('INOUT').getValue() == "1") {
                    //    f2.findField('LOT_NO').show();
                    //    f2.findField('LOT_NO_N').hide();
                    //}
                    //else {
                    //    f2.findField('LOT_NO').show();
                    //    f2.findField('LOT_NO_N').show();
                    //}
                    var apvqty = 0;
                    var totprice = 0;
                    var contprice = Number(r.get('M_CONTPRICE'));
                    var sum_ex = Number(T2Query.getForm().findField('SUM_EX').getValue());
                    var sum_in = Number(T2Query.getForm().findField('SUM_IN').getValue());
                    if (T1F4 == '2' && this.up('form').getForm().findField('INOUT').getValue() == '1') {
                        apvqty = parseInt((sum_ex - sum_in) / contprice) + 1;
                        totprice = apvqty * contprice;
                        f.findField("APVQTY").setValue(apvqty);
                        f.findField("C_AMT").setValue(totprice);
                    }
                }
                else {
                    f.findField("MMCODE").setValue("");
                    f.findField("MMNAME_C").setValue("");
                    f.findField("MMNAME_E").setValue("");
                    f.findField("AGEN_NAMEC").setValue("");
                    f.findField("BASE_UNIT").setValue("");
                    f.findField('IN_PRICE').setValue("");
                    f.findField('CONTPRICE').setValue("");
                    Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
                    Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
                    DISCPERC = '';
                    CONTPRICE = '';
                }
                f.findField("LOT_NO").setValue("");
                f.findField("LOT_NO").clearInvalid();
                f.findField("EXP_DATE").setValue("");
                f.findField("INV_QTY").setValue("");
                //f.findField("APVQTY").setValue("");
                //f.findField("APVQTY").clearInvalid();
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
            name: 'EXP_DATE',
            xtype: 'hidden'
        }, {
            name: 'C_UP',
            xtype: 'hidden'
        }, {
            name: 'M_CONTPRICE',
            xtype: 'hidden'
        }, {
            xtype: 'combo',
            fieldLabel: '換入/換出',
            name: 'INOUT',
            store: st_stat,
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
            fieldCls: 'required',
            listeners: {
                select: function (c, r, i, e) {
                    var f2 = T2Form.getForm();
                    if (r.get('VALUE') == "1") {
                        //f2.findField('LOT_NO').show();
                        //f2.findField('LOT_NO_N').hide();
                        T2Form.down('#btnLotno').setVisible(false);
                    }
                    else {
                        //f2.findField('LOT_NO').hide();
                        //f2.findField('LOT_NO_N').show();
                        T2Form.down('#btnLotno').setVisible(true);
                    }
                    if (r.get('VALUE') == "1" && T1F4 == "2" && T2Store.data.length == 0) {
                        Ext.Msg.alert('提醒', "等值更換必須先執行<span style='color:red'>換出</span>。");
                        f2.findField('INOUT').setValue("2");
                    }
                }
            }
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
                            popMmcodeForm(viewport, '/api/AA0025/GetMmcode', {
                                MMCODE: f.findField("MMCODE").getValue(),
                                MAT_CLASS: T1Form.getForm().findField('MAT_CLASS').getValue(),
                                AGEN_NO: '',
                                INOUT: T2Form.getForm().findField('INOUT').getValue()
                            }, setMmcode);
                        }
                    }
                }

            ]
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商', //白底顯示
            name: 'AGEN_NAMEC'
        }, {
            xtype: 'container',
            layout: 'hbox',
            padding: '0 7 7 0',
            items: [
                //{
                //    xtype: 'combo',
                //    fieldLabel: '批號',
                //    name: 'LOT_NO_N',
                //    store: st_lotno,
                //    queryMode: 'local',
                //    displayField: 'TEXT',
                //    valueField: 'VALUE',
                //    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                //    anyMatch: true,
                //    readOnly: true,
                //    //allowBlank: false, // 欄位為必填
                //    typeAhead: true,
                //    forceSelection: true,
                //    queryMode: 'local',
                //    triggerAction: 'all',
                //    fieldCls: 'required',
                //    listeners: {
                //        select: function (c, r, i, e) {
                //            var f = T2Form.getForm();
                //            f.findField('EXP_DATE_T').setValue(r.get('EXTRA1'));
                //            f.findField('EXP_DATE').setValue(r.get('EXTRA1'));
                //            f.findField('INV_QTY').setValue(r.get('EXTRA2'));
                //            f.findField('LOT_NO').setValue(r.get('VALUE'));
                //        }
                //    }
                //},
                {
                    xtype: 'hidden',
                    name: 'LOT_NO_N'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '批號',
                    name: 'LOT_NO',
                    width: 220,
                    allowBlank: false,
                    enforceMaxLength: true,
                    maxLength: 20,
                    fieldCls: 'required',
                    readOnly: true
                },
                {
                    xtype: 'button',
                    itemId: 'btnLotno',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T2Form.getForm();
                        if (!f.findField("LOT_NO").readOnly) {
                            popLotnoForm(viewport, '/api/AA0025/GetLotno', {
                                MMCODE: f.findField("MMCODE").getValue()
                            }, setLotno);
                        }
                    }
                }
                
            ]
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            name: 'EXP_DATE_T',
            fieldCls: 'required',
            allowBlank: false, // 欄位為必填
            readOnly: true
        //}, {
        //    xtype: 'displayfield',
        //    fieldLabel: '效期',
        //    name: 'EXP_DATE_T'
        }, {
            xtype: 'displayfield',
            fieldLabel: '效期數量',
            name: 'INV_QTY'
        }, {
            fieldLabel: '換貨數量',
            name: 'APVQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                change: function (field, newValue, oldValue) {
                    //var objP0 = T2Form.getForm().findField("APVQTY");
                    //var objP1 = T2Form.getForm().findField("M_CONTPRICE");
                    //var objP5 = T2Form.getForm().findField("C_AMT");
                    //if (objP0.getValue() != '' && objP1.getValue() != '') {
                    //    objP5.setValue(parseFloat(objP0.getValue()) * parseFloat(objP1.getValue()) );
                    //}
                    NEW_APVQTY = newValue;
                    if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == true) &&
                        (T2Form.getForm().findField("CONTPRICE_1").getValue() == false)) {
                        T2Form.getForm().findField("C_AMT").setValue(Math.round(DISCPERC * NEW_APVQTY * 100) / 100);
                    } if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == false) &&
                        (T2Form.getForm().findField("CONTPRICE_1").getValue() == true)) {
                        T2Form.getForm().findField("C_AMT").setValue(Math.round(CONTPRICE * NEW_APVQTY * 100) / 100);
                    }
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '單位',
            name: 'BASE_UNIT'
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨單價', //白底顯示
            name: 'IN_PRICE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '合約單價', //白底顯示
            name: 'CONTPRICE',
            submitValue: true
        },
            {
                xtype: 'radiogroup',
                name: 'C_TYPE',
                id: 'C_TYPE',
                fieldLabel: '單價',
                labelWidth: 65,
                width: 200,
                columns: 1,
                fieldCls: 'required',
                allowBlank: false,
                items: [
                    { boxLabel: '進貨單價', id: 'IN_PRICE_1', inputValue: '1', width: 150 },
                    { boxLabel: '合約單價', id: 'CONTPRICE_1', inputValue: '2', width: 150, checked: true },
                    { boxLabel: '其他', id: 'OTHER', inputValue: '3', width: 150, hidden: true }
                ],
                listeners: {
                    change: function (field, newValue, oldValue) {
                        if (newValue.C_TYPE == '1') {
                            T2Form.getForm().findField("C_AMT").setValue(Math.round(DISCPERC * NEW_APVQTY * 100) / 100);
                            T2Form.getForm().findField("C_UP").setValue(DISCPERC);
                            T2Form.getForm().findField("M_CONTPRICE").setValue(DISCPERC);
                        }
                        else if (newValue.C_TYPE == '2') {
                            T2Form.getForm().findField("C_AMT").setValue(Math.round(CONTPRICE * NEW_APVQTY * 100) / 100);
                            T2Form.getForm().findField("C_UP").setValue(CONTPRICE);
                            T2Form.getForm().findField("M_CONTPRICE").setValue(CONTPRICE);
                        }
                    }
                }
            }, {
            xtype: 'displayfield',
            fieldLabel: '換貨金額',
            name: 'C_AMT'
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'ITEM_NOTE',
            enforceMaxLength: true,
            maxLength: 50,
            height: 200,
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("MMCODE").getValue()) == "" ||
                    (T2Form.getForm().findField("MMCODE").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("LOT_NO").getValue()) == "" ||
                    (T2Form.getForm().findField("LOT_NO").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>批號不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>批號不可為空</span>，請重新輸入。");
                } else if ((T2Form.getForm().findField("APVQTY").getValue()) == "" ||
                    (T2Form.getForm().findField("APVQTY").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>換貨量不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>換貨量不可為空</span>，請重新輸入。");
                } else if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == false) &&
                    (T2Form.getForm().findField("CONTPRICE_1").getValue() == false)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>單價需二擇一</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>單價需二擇一</span>，請重新輸入。");
                }
                else if (T2Form.getForm().findField("C_TYPE").getValue().C_TYPE == '3') {
                    Ext.Msg.alert('提醒', "<span style='color:red'>單價需二擇一</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>單價需二擇一</span>，請重新輸入。");
                }
                else {
                    if ((this.up('form').getForm().findField('LOT_NO').getValue() == '' || this.up('form').getForm().findField('LOT_NO').getValue() == null)
                        || (this.up('form').getForm().findField('EXP_DATE_T').getValue() == '' || this.up('form').getForm().findField('EXP_DATE_T').getValue() == null)) {
                        Ext.Msg.alert('提醒', '批號及效期為必填');
                    }
                    else {
                        if (this.up('form').getForm().findField('APVQTY').getValue() == '0')
                            Ext.Msg.alert('提醒', '換貨數量不可為0');
                        else {
                            var invqty = Number(this.up('form').getForm().findField('INV_QTY').getValue());
                            var apvqty = Number(this.up('form').getForm().findField('APVQTY').getValue());
                            var c_amt = Number(this.up('form').getForm().findField('C_AMT').getValue());
                            var sum_ex = Number(T2Query.getForm().findField('SUM_EX').getValue());
                            var sum_in = Number(T2Query.getForm().findField('SUM_IN').getValue());
                            if (this.up('form').getForm().findField('INOUT').getValue() == '3' && apvqty > invqty) {
                                Ext.Msg.alert('提醒', '換貨數量不可超過效期數量');
                            }
                            else if (T1F4 == '2' && this.up('form').getForm().findField('INOUT').getValue() == '1' && sum_ex > sum_in + c_amt) {
                                Ext.Msg.alert('提醒', "<span style='color:red'>換入金額小於換出金額。</span>");
                                msglabel(" <span style='color:red'>換入金額小於換出金額。</span>");
                            }
                            else {
                                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        T2Submit();
                                    }
                                }
                                );
                            }
                        }
                    }
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
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
                    f2.findField('EXP_DATE').setValue(f2.findField('EXP_DATE_T').getValue());
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
        f.findField('APVQTY').setReadOnly(true);
        f.findField('ITEM_NOTE').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        T2Form.down('#btnMmcode').setVisible(false);
        T2Form.down('#btnLotno').setVisible(false);
        viewport.down('#form').setTitle('瀏覽');
        Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
        Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
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
                    T11Set = '../../../api/AA0025/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '../../../api/AA0025/UpdateD';
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
                                    url: '/api/AA0025/DeleteD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            msglabel('資料刪除成功');
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
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            if (T1F4 == "2") {
                f2.findField("INOUT").setValue('2');
            }
            else {
                f2.findField("INOUT").setValue('1');
            }
            f2.findField("APVQTY").setValue('0');
            f2.findField("INV_QTY").setValue('0');
            //f2.findField('LOT_NO').show();
            //f2.findField('LOT_NO_N').hide();
            T2Form.down('#btnMmcode').setVisible(true);
            if (f2.findField('INOUT').getValue() == '2') {
                T2Form.down('#btnLotno').setVisible(true);
            }
            else {
                T2Form.down('#btnLotno').setVisible(false);
            }
            Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
            Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
            //u.setReadOnly(false);
        }
        else {
            u = f2.findField('APVQTY');
            SetM_CONTPRICE();
            SetM_DISCPERC();
            T2Form.getForm().findField("APVQTY").setValue(T2Form.getForm().findField("INV_QTY").getValue());
            SetLOT_NO(T2Form.getForm().findField('MMCODE').getValue());
        }

        f2.findField('x').setValue(x);
        f2.findField('INOUT').setReadOnly(false);
        f2.findField('MMCODE').setReadOnly(false);
        f2.findField('LOT_NO').setReadOnly(false);
        f2.findField('LOT_NO_N').setReadOnly(false);
        f2.findField('APVQTY').setReadOnly(false);
        f2.findField('EXP_DATE_T').setReadOnly(false);
        f2.findField('ITEM_NOTE').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        //if (f2.findField('INOUT').getValue() == "1") {
        //    f2.findField('LOT_NO').show();
        //    f2.findField('LOT_NO_N').hide();
        //}
        //else {
        //    f2.findField('LOT_NO').hide();
        //    f2.findField('LOT_NO_N').show();
        //}
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
        },{
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
            text: "換入/換出",
            dataIndex: 'INOUT_N',
            width: 80,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180,
            sortable: true
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NAMEC',
            width: 170
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 80
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE_T',
            width: 80
        }, {
            text: "換貨數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
            text: "單價",
            dataIndex: 'C_UP',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "換貨金額",
            dataIndex: 'C_AMT',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "備註",
            dataIndex: 'ITEM_NOTE',
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
            var f2 = T2Form.getForm();
            f2.findField('x').setValue('U');
            T2F1 = f2.findField('INOUT').getValue();
            if (T1F3 === '0801') {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
            }
            else {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
            //if (T2F1 == "1") {
            //    f2.findField('LOT_NO').show();
            //    f2.findField('LOT_NO_N').hide();
            //}
            //else {
            //    st_lotno.load();
            //    f2.findField('LOT_NO').hide();
            //    f2.findField('LOT_NO_N').show();
            //}
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
                fieldLabel: '換貨日期區間',
                labelAlign: 'right',
                name: 'T3P0',
                id: 'T3P0',
                vtype: 'dateRange',
                dateRange: { end: 'T3P1' },
                padding: '0 4 0 4',
                labelWidth: 80,
                width: 170,
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: 10,
                name: 'T3P1',
                id: 'T3P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T3P0' },
                padding: '0 4 0 4',
                width: 90,
                value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T3print', text: '列印', handler: function () {
                showReport();
            }
        },
        {
            itemId: 'T3cancel', text: '取消', handler: hideWin3
        }
        ]
    });
    

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '列印',
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
    function showReport() {

        var qstring = '?p0=' + T3Form.getForm().findField('T3P0').rawValue + '&p1=' + T3Form.getForm().findField('T3P1').rawValue ;
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        hideWin3();
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
                xtype: 'datefield',
                fieldLabel: '換貨日期區間',
                labelAlign: 'right',
                name: 'T4P0',
                id: 'T4P0',
                vtype: 'dateRange',
                dateRange: { end: 'T4P1' },
                padding: '0 4 0 4',
                labelWidth: 80,
                width: 170,
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: 10,
                name: 'T4P1',
                id: 'T4P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T4P0' },
                padding: '0 4 0 4',
                width: 90,
                value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T4print', text: '列印', handler: function () {
                showReportB();
            }
        },
        {
            itemId: 'T4cancel', text: '取消', handler: hideWin4
        }
        ]
    });


    var win4;
    var winActWidth4 = 300;
    var winActHeight4 = 200;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '列印換貨清單',
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
    function showReportB() {

        var qstring = '?p0=' + T4Form.getForm().findField('T4P0').rawValue + '&p1=' + T4Form.getForm().findField('T4P1').rawValue;
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportBUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        hideWin4();
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
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    viewport.down('#form').collapse();
});
