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
    var T1Name = '報廢作業';
    var userId, userName, userInid, userInidName, userTaskId;
    var reportUrl = '/Report/A/AA0162.aspx';
    //效期
    var GetLOT_NO = '../../../api/AA0151/GetLOT_NO';
    var GetINV_QTY = '../../../api/AA0151/GetINV_QTY';

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

                        //計算報廢金額
                        var pCUP = T2Form.getForm().findField('C_UP').getValue();
                        if (isNaN(Number(pCUP)) || pCUP == '')
                            pCUP = 0;
                        var pAPVQTY = T2Form.getForm().findField('APVQTY').getValue();
                        if (isNaN(Number(pAPVQTY)) || pAPVQTY == '')
                            pAPVQTY = 0;
                        T2Form.getForm().findField("C_AMT").setValue(pCUP * pAPVQTY)

                    }
                }
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
                var invQty = Ext.decode(response.responseText)
                T2Form.getForm().findField("INV_QTY").setValue(invQty);
            },
            failure: function (response, options) {
            }
        });
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPDEPT_NAME', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MAT_CLASS', direction: 'DESC' }, { property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0162/AllM',
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
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'C_UP', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'C_AMT', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'ITEM_NOTE', type: 'string' }
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
            url: '/api/AA0162/AllD',
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
            T2Store.getProxy().setExtraParam("p1", T1LastRec.data["FRWH"]);
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
            url: '/api/AA0162/GetMclassQCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {

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
            url: '/api/AA0162/GetMclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {

            }
        },
        autoLoad: true
    });

    var FlowidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0162/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var UserInfoStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/Acct/Info',
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
                fieldLabel: '報廢單號',
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
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
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
                fieldLabel: '報廢單狀態',
                name: 'P2',
                id: 'P2',
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4',
                store: FlowidStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                multiSelect: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
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
                triggerAction: 'all'
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
                    Ext.getCmp('btnExp').setDisabled(true);
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

    function setSubmit(args) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0162/UpdateStatusBySP',
            method: reqVal_p,
            params: {
                DOCNO: args.DOCNO
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
                    Ext.getCmp('btnExp').setDisabled(true);
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
                    T1Set = '/api/AA0162/MasterCreate';
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
                    T1Set = '/api/AA0162/MasterUpdate';
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
                                    url: '/api/AA0162/MasterDelete',
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

                        Ext.MessageBox.confirm('報廢', '請確認是否過帳？' + '單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                setSubmit({
                                    DOCNO: docno
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '效期',
                id: 'btnExp',
                name: 'btnExp',
                disabled: true,
                hidden: true,  //1130319改由user輸入批號效期
                handler: function () {
                    showExpWindow(T1LastRec.data.DOCNO, 'O', viewport);
                }
            },
            {
                itemId: 'print', id: 'print', text: '列印', handler: function () {
                    showReport();
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p1: T1Query.getForm().findField('P1').getValue(),
                d0: T1Query.getForm().findField('D0').rawValue,
                d1: T1Query.getForm().findField('D1').rawValue,
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?d0=' + np.d0 + '&d1=' + np.d1 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        Tabs.setActiveTab('Form');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("MAT_CLASS");
            // u.setReadOnly(false);
            u.clearInvalid();
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userName);
            f.findField("APPDEPT_NAME").setValue(userInid + ' ' + userInidName);

            var year = new Date().getFullYear();
            var month = (new Date().getMonth() + 1) > 9 ? (new Date().getMonth() + 1).toString() : "0" + (new Date().getMonth() + 1).toString();
            var date = (new Date().getDate()) > 9 ? new Date().getDate().toString() : "0" + new Date().getDate().toString();
            var chtToday = (year - 1911).toString() + month.toString() + date.toString();
            f.findField("APPTIME").setValue(chtToday);
        }
        else {
            f.findField('MAT_CLASS').setReadOnly(false);
            u = f.findField("APPLY_NOTE");
        }
        f.findField('APPLY_NOTE').setReadOnly(false);
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
                xtype: 'displayfield',
                fieldLabel: '報廢單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '報廢單位',
                name: 'APPDEPT_NAME',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
                name: 'APPTIME',
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
                        var f = T1Form.getForm();
                        f.findField('FRWH').setValue(record.get("EXTRA2"));
                    }
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '出庫庫房',
                name: 'FRWH_N',
                readOnly: true
            },
            {
                name: 'FRWH',
                xtype: 'hidden',
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: '申請單備註',
                name: 'APPLY_NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
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
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 100,
                renderer: function (value) {
                    return value.split(' ')[1];
                }
            },
            {
                text: "報廢單號",
                dataIndex: 'DOCNO',
                width: 130
            },
            {
                text: "申請單狀態",
                dataIndex: 'FLOWID',
                width: 90,
                renderer: function (value) {
                    return value.split(' ')[1];
                }
            },
            {
                name: 'FRWH',
                xtype: 'hidden'
            },
            {
                text: "出庫庫房",
                dataIndex: 'FRWH_N',
                width: 170
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 90
            },
            {
                text: "申請人員",
                dataIndex: 'APPID',
                width: 130
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
            var showExp = true;
            var selection = T1Grid.getSelection();
            if (selection.length) {
                if (selection.length > 1) {
                    canEdit = false; // 勾選資料大於1筆則不可使用編輯功能
                    showExp = false;
                }
                $.map(selection, function (item, key) {
                    if (item.get('FLOWID').split(' ')[0] != '0501' && item.get('FLOWID').split(' ')[0] != '1') {
                        canEdit = false; // 有任一筆不是未過帳/報廢申請中則不可過帳和編輯
                        canSubmit = false;
                    }
                    if (item.get('TOTAL') == 0)
                        showExp = false;
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

            if (showExp)
                Ext.getCmp('btnExp').setDisabled(false);
            else
                Ext.getCmp('btnExp').setDisabled(true);

            Ext.getCmp('btnUpdate2').setDisabled(true);
            Ext.getCmp('btnDel2').setDisabled(true);
        }
        else {
            Ext.getCmp('btnUpdate').setDisabled(true);
            Ext.getCmp('btnDel').setDisabled(true);
            Ext.getCmp('btnExp').setDisabled(true);
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
            f.findField("APPID").setValue(T1LastRec.data["APPID"]);
            f.findField("APPDEPT_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("FRWH_N").setValue(T1LastRec.data["FRWH_N"]); //寫入庫房名稱
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]); //寫入庫房代碼
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"].split(' ')[0]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);

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
                    T1Set = '/api/AA0162/DetailCreate';
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
                    T1Set = '/api/AA0162/DetailUpdate';
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
                                    url: '/api/AA0162/DetailDelete',
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
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField('MMCODE').setReadOnly(false);
            u = f.findField("MMCODE");
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField("MMNAME_C").setValue(T2LastRec.data["MMNAME_C"]);
            f.findField("MMNAME_E").setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            f.findField("C_UP").setValue(T2LastRec.data["C_UP"]);
            f.findField("C_AMT").setValue(T2LastRec.data["C_AMT"]);
            f.findField("LOT_NO").setValue(T2LastRec.data["LOT_NO"]);
            f.findField("EXP_DATE_T").setValue(T2LastRec.data["EXP_DATE_T"]);
            f.findField("BASE_UNIT").setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField("ITEM_NOTE").setValue(T2LastRec.data["ITEM_NOTE"]);

            SetINV_QTY(); //取得效期數量，要存入INV_QTY
        }
        f.findField('x').setValue(x);
        f.findField('APVQTY').setReadOnly(false);
        f.findField('ITEM_NOTE').setReadOnly(false);

        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE_T').setReadOnly(false);

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
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 80,
                sortable: true
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE_T',
                width: 80,
                sortable: true
            },
            {
                text: "數量",
                dataIndex: 'APVQTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單價",
                dataIndex: 'C_UP',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "報廢金額",
                dataIndex: 'C_AMT',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "備註",
                dataIndex: 'ITEM_NOTE',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        Ext.getCmp('btnUpdate2').setDisabled(T2Rec === 0);
        Ext.getCmp('btnDel2').setDisabled(T2Rec === 0);
        if (T2LastRec != null) {
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField("LOT_NO").setValue(T2LastRec.data["LOT_NO"]);
            f.findField("EXP_DATE_T").setValue(T2LastRec.data["EXP_DATE_T"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            f.findField('C_UP').setValue(T2LastRec.data["C_UP"]);
            f.findField('C_AMT').setValue(T2LastRec.data["C_AMT"]);
            f.findField('ITEM_NOTE').setValue(T2LastRec.data["ITEM_NOTE"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APVQTY').setReadOnly(true);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);

            if (T1Rec == 1 && (T1LastRec.data["FLOWID"].split(' ')[0] === '0501' || T1LastRec.data["FLOWID"].split(' ')[0] == '1')) {
                Ext.getCmp('btnUpdate2').setDisabled(false);
                Ext.getCmp('btnDel2').setDisabled(false);
            }
            else {
                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnDel2').setDisabled(true);
            }
        }

    }

    function T1Cleanup() {
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
        f.findField('APPLY_NOTE').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
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
        matchFieldWidth: false,
        listConfig: { width: 180 },
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0162/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MAT_CLASS', 'BASE_UNIT', 'DISC_CPRICE'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                return {
                    DOCNO: T1LastRec.data["DOCNO"]
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                f.findField("LOT_NO").setValue("");
                f.findField("LOT_NO").clearInvalid();
                f.findField("EXP_DATE").setValue("");
                f.findField("INV_QTY").setValue("");

               //選取下拉項目時，顯示回傳值
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                    f.findField("C_UP").setValue(r.get('DISC_CPRICE')); //設定單價
                    SetLOT_NO(r.get('MMCODE'));
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
            labelWidth: 80,
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
                xtype: 'hidden',
                name: 'DOCNO',

            },
            {
                xtype: 'hidden',
                name: 'WH_NO',

            },
            {
                xtype: 'hidden',
                name: 'SEQ',

            },
            {
                name: 'EXP_DATE',
                xtype: 'hidden'
            },
            {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
                    T2FormMMCode,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            if (!f.findField("MMCODE").readOnly) {
                                popMmcodeForm_14(viewport, '/api/AA0162/GetMmcode', {
                                    DOCNO: T1LastRec.data["DOCNO"],
                                    MMCODE: f.findField("MMCODE").getValue()
                                }, setMmcode);
                            }
                        }
                    }
                ]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                enforceMaxLength: true,
                readOnly: true,
                labelWidth: 80,
                labelAlign: 'right'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                enforceMaxLength: true,
                readOnly: true,
                labelWidth: 80,
                labelAlign: 'right'
            },
            {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
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
                                popLotnoForm(viewport, '/api/AA0151/GetLotno', {
                                    MMCODE: f.findField("MMCODE").getValue(),
                                    DOCNO: f.findField("DOCNO").getValue()
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
            },
            {
                xtype: 'hidden',
                name: 'INV_QTY' //儲存效期數量
            },
            {
                xtype: 'numberfield',
                fieldLabel: '數量',
                name: 'APVQTY',
                enforceMaxLength: true,
                submitValue: true,
                fieldCls: 'required',
                hideTrigger: true,
                readOnly: true,
                minValue: 0,
                labelWidth: 80,
                labelAlign: 'right',
                allowBlank: false,
                onChange: function (newValue, oldValue) {
                    //重新計算報廢金額
                    var pCUP = T2Form.getForm().findField('C_UP').getValue();
                    if (isNaN(Number(pCUP)) || pCUP == '')
                        pCUP = 0;
                    var pAPVQTY = newValue;
                    if (isNaN(Number(pAPVQTY)) || pAPVQTY == '')
                        pAPVQTY = 0;
                    if (pCUP != 0 && pAPVQTY != 0) {
                        T2Form.getForm().findField("C_AMT").setValue(pCUP * pAPVQTY);
                    }
                    
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單價',
                name: 'C_UP',
                readOnly: true,
                labelWidth: 80,
                labelAlign: 'right',
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單位',
                name: 'BASE_UNIT',
                readOnly: true,
                labelWidth: 80,
                labelAlign: 'right'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '報廢金額',
                name: 'C_AMT',
                readOnly: true,
                labelWidth: 80,
                labelAlign: 'right'
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'ITEM_NOTE',
                labelWidth: 80,
                labelAlign: 'right',
                readOnly: true,
                allowBlank: true,
                maxLength: 50,
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {

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
                        Ext.Msg.alert('提醒', "<span style='color:red'>數量不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>數量不可為空</span>，請重新輸入。");
                    } else {
                        if ((this.up('form').getForm().findField('LOT_NO').getValue() == '' || this.up('form').getForm().findField('LOT_NO').getValue() == null)
                            || (this.up('form').getForm().findField('EXP_DATE_T').getValue() == '' || this.up('form').getForm().findField('EXP_DATE_T').getValue() == null)) {
                            Ext.Msg.alert('提醒', '批號及效期為必填');
                        }
                        else {
                            //檢查數量是否合理 <= 效期數量
                            var pINVQTY = T2Form.getForm().findField("INV_QTY").getValue();
                            if (isNaN(Number(pINVQTY)) || pINVQTY == '')
                                pINVQTY = 0;
                            var pAPVQTY = T2Form.getForm().findField("APVQTY").getValue();
                            if (isNaN(Number(pAPVQTY)) || pAPVQTY == '')
                                pAPVQTY = 0;

                            if (this.up('form').getForm().findField('APVQTY').getValue() == '0'){
                                Ext.Msg.alert('提醒', '數量不可為0');
                            } else if (pAPVQTY > pINVQTY) {
                                Ext.Msg.alert('提醒', '數量不可超過效期數量');
                            } else {
                                //驗證完成
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
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield" || fc.xtype === "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
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

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
});