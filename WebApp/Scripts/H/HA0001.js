Ext.onReady(function () {
    Ext.Loader.setConfig({
        enabled: true,
        paths: {
            'WEBAPP': '/Scripts/app'
        }
    });

    var T1Get = '/api/HA0001/All';
    var T1Set = '/api/HA0001/Update';
    var banknameGet = '/api/HA0001/GetBankname';
    var hospbankGet = '/api/HA0001/GetHospbank';
    var isremitChk = '/api/HA0001/ChkIsremit';
    var bankChk = '/api/HA0001/ChkBank';
    var reloadAB = '/api/HA0001/LoadAgenBank';
    var T1GetTxt = '/api/HA0001/GetTxt';
    var T1GetChkTxt = '/api/HA0001/GetChkTxt';
    var T1GetExcel = '/api/HA0001/Excel';
    var sendMail = '/api/HA0001/sendMail';
    var amtGet = '/api/HA0001/calcAmtMsg';
    var chkQuery = '/api/HA0001/ChkQuery';
    var reportUrl = '/Report/H/HA0001.aspx';
    var T1Name = '主計作業';

    var T1Rec = 0;
    var T1LastRec = null;

    var PRINT_TYPE_store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'DESC'],
        data: [
            { 'VALUE': '0', 'DESC': '全部' },
            { 'VALUE': '1', 'DESC': '已匯款' },
            { 'VALUE': '2', 'DESC': '未匯款' }
        ]
    });

    function getInitConfig() {
        Ext.Ajax.request({
            url: '/api/HA0001/ChkBtnGroup',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    // 部分醫院依使用者群組判斷顯示的按鈕
                    if (data.msg == 'MACC') {
                        // 主計
                        T1Grid.down('#export').setVisible(false);
                        T1Grid.down('#sendMail').setVisible(false);
                    }
                    else if (data.msg == 'MMSPL') {
                        // 衛保室
                        //T1Grid.down('#edit').setVisible(false);
                        T1Grid.down('#reloadAgenBank').setVisible(false);
                        T1Grid.down('#downloadForm').setVisible(false);
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

    function T1Load() {
        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();
        msglabel('');
        T1Tool.moveFirst();
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();

        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            if (f.xtype === "combo" || f.xtype === "datefield") {
                f.readOnly = true;
            }
            else {
                f.setReadOnly(true);
            }
        });

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'REMITNO', type: 'string' },
            { name: 'DATA_YM', type: 'string' },
            { name: 'REMITDATE', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAME', type: 'string' },
            { name: 'AGEN_TEL', type: 'string' },
            { name: 'AGEN_ADD', type: 'string' },
            { name: 'AGEN_BANK_14', type: 'string' },
            { name: 'BANKNAME', type: 'string' },
            { name: 'AGEN_ACC', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'ADDORSUB_AMT', type: 'string' },
            { name: 'AMTPAYABLE', type: 'string' },
            { name: 'DISC_AMT', type: 'string' },
            { name: 'REBATE_AMT', type: 'string' },
            { name: 'AMTPAID', type: 'string' },
            { name: 'PROCFEE', type: 'string' },
            { name: 'MGTFEE', type: 'string' },
            { name: 'REMIT', type: 'string' },
            { name: 'ISREMIT', type: 'string' },
            { name: 'XFRMEMO', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10000,
        //autoLoad: true,
        remoteSort: true,
        sorters: [{ property: 'REMITNO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

                T1Query.down('#T1Query_AmtMsg').setText('總匯款量：');
            },
            load: function (store, records, successful, eOpts) {
                T1Cleanup();
                if (records.length == 0) {
                    T1Grid.down('#downloadForm').setDisabled(true);
                    T1Grid.down('#reportForm').setDisabled(true);
                    T1Grid.down('#sendMail').setDisabled(true);
                    
                    Ext.Msg.alert('訊息', '月結月份 ' + T1Query.getForm().findField('P0').rawValue + ' 沒有可轉入的資料');
                }
                else {
                    T1Grid.down('#downloadForm').setDisabled(false);
                    T1Grid.down('#reportForm').setDisabled(false);
                    T1Grid.down('#sendMail').setDisabled(false);


                    calAmtMsg();
                }
            }
        },
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
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        padding: 3,
        autoScroll: true,
        border: false,
        items: [{
            xtype: 'panel',
            border: false,
            defaultType: 'textfield',
            padding: '0 0 4 0',
            layout: 'hbox',
            defaults: {
                labelAlign: "right",
                labelWidth: 40
            },
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '月結月份',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 5,
                    width: 150,
                    labelWidth: 80,
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getDefaultValue(),
                    //regexText: '需以5位數字表示年月',
                    //regex: /^[0-9]{5}$/
                }, {
                    fieldLabel: '廠商代碼或名稱',
                    name: 'P1',
                    id: 'P1',
                    width: 250,
                    labelWidth: 130
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            queryChk();
                        }
                        else {
                            Ext.Msg.alert('訊息', '查詢條件有誤');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('');
                    }
                }, {
                    xtype: 'label',
                    text: '',
                    padding: '4 4 0 20',
                    name: 'T1Query_AmtMsg',
                    id: 'T1Query_AmtMsg',
                    width: 600
                }
            ]
        }]
    });

    var T1FormColWidth = 250;
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 3
        },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right",
            width: T1FormColWidth
        },
        items: [{
            xtype: 'displayfield',
            fieldLabel: '匯款單編號',
            name: 'REMITNO',
            readOnly: true,
            submitValue: true,
            colspan: 3,
            width: T1FormColWidth * 3
        }, {
            xtype: 'displayfield',
            fieldLabel: '月結月份',
            name: 'DATA_YM',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '匯款日期',
            name: 'REMITDATE',
            readOnly: true
        }, {
            xtype: 'label',
            text: '(當電匯後,此日期會以電匯日期為主)'
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商代碼',
            name: 'AGEN_NO',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商名稱',
            hideLabel: true,
            name: 'AGEN_NAME',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商電話',
            name: 'AGEN_TEL',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商地址',
            name: 'AGEN_ADD',
            readOnly: true,
            colspan: 3,
            width: T1FormColWidth * 3
        }, {
            fieldLabel: '匯款銀行代碼',
            name: 'AGEN_BANK_14',
            enforceMaxLength: true,
            maxLength: 7,
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9]/,
            // allowBlank: false,
            allowOnlyWhitespace: false,
            // fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    Ext.Ajax.request({
                        url: banknameGet,
                        method: reqVal_p,
                        params: {
                            p0: T1Form.getForm().findField('AGEN_BANK_14').getValue()
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T1Form.getForm().findField('BANKNAME').setValue(data.msg);
                            } else {
                                Ext.MessageBox.alert('錯誤', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '銀行名稱',
            hideLabel: true,
            name: 'BANKNAME',
            readOnly: true,
            colspan: 2,
            width: T1FormColWidth * 2
        }, {
            fieldLabel: '匯款帳號',
            name: 'AGEN_ACC',
            enforceMaxLength: true,
            maxLength: 20,
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9]/,
            // allowBlank: false,
            // allowOnlyWhitespace: false,
            // fieldCls: 'required',
            readOnly: true,
            colspan: 3
        }, {
            fieldLabel: '列帳金額',
            name: 'PO_AMT',
            regexText: '只能輸入數字,至多小數2位',
            regex: /^[0-9]+(\.[0-9]{1,2})?$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9.]/,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAMT();
                }
            }
        }, {
            fieldLabel: '增減金額',
            name: 'ADDORSUB_AMT',
            regexText: '只能輸入數字,至多小數2位', // 可負數
            regex: /^\-?[0-9]+(\.[0-9]{1,2})?$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9.-]/,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAMT();
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '<font color="green">應付金額</font>',
            name: 'AMTPAYABLE',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'label',
            text: ' '
        }, {
            xtype: 'label',
            text: '(增減金額為正數表示應補給金額,為負數為應再扣匯款金額)',
            colspan: 2,
            width: T1FormColWidth * 2
        }, {
            fieldLabel: '合約優惠款',
            name: 'DISC_AMT',
            regexText: '只能輸入數字,至多小數2位',
            regex: /^[0-9]+(\.[0-9]{1,2})?$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9.]/,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAMT();
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '<font color="purple">實付金額</font>',
            name: 'AMTPAID',
            readOnly: true,
            colspan: 2,
            submitValue: true
        }, {
            fieldLabel: '匯款匯費',
            name: 'PROCFEE',
            regexText: '只能輸入數字,至多小數2位',
            regex: /^[0-9]+(\.[0-9]{1,2})?$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9.]/,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAMT();
                }
            }
        }, {
            fieldLabel: '手續費',
            name: 'MGTFEE',
            regexText: '只能輸入數字,至多小數2位',
            regex: /^[0-9]+(\.[0-9]{1,2})?$/, // 用正規表示式限制可輸入內容
            maskRe: /[0-9.]/,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAMT();
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '<font color="orange">匯款總金額</font>',
            name: 'REMIT',
            readOnly: true,
            submitValue: true
        }, {
            fieldLabel: '附言',
            name: 'XFRMEMO',
            readOnly: true,
            enforceMaxLength: true,
            maxLength: 200,
            colspan: 3,
            width: T1FormColWidth * 3
        }, {
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                var x = f2.findField("x").getValue();
                switch (x) {
                    case "U":
                        msglabel('修改成功');
                        T1Load();
                        break;
                }
                myMask.hide();
                T1Cleanup();
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                }
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        //width: '100%',
        //frame: false,
        border: false,
        plain: true,
        buttons: [
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        setFormT1("U", '修改');
            //    }
            //},
            {
                itemId: 'reloadAgenBank', text: '載入廠商匯款資料', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('重新載入-廠商代碼' + T1LastRec.data.AGEN_NO, '是否確定從廠商基本資料重新載入匯款資料？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: reloadAB,
                                method: reqVal_p,
                                params: {
                                    p0: T1LastRec.data.REMITNO,
                                    p1: T1LastRec.data.AGEN_NO
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.MessageBox.alert('訊息', '載入完成');
                                        T1Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {
                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                itemId: 'downloadForm', text: '轉電匯資料', disabled: true,
                handler: function () {
                    popDownloadFormWin(T1Query.getForm().findField('P0').rawValue);
                }
            },
            {
                itemId: 'export', text: '下載匯款通知單', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1LastRec.data.DATA_YM });
                    p.push({ name: 'p1', value: T1LastRec.data.REMITNO });
                    PostForm(T1GetExcel, p);
                }
            },
            {
                itemId: 'sendMail', text: '寄送匯款通知單', disabled: true,
                handler: function () {
                    //Ext.MessageBox.confirm('寄送匯款通知單', '是否確定寄送匯款通知單？<br>匯款單編號：<font color="blue">' + T1LastRec.data.REMITNO + '</font><br>廠商：<font color="blue">' + T1LastRec.data.AGEN_NO + ' ' + T1LastRec.data.AGEN_NAME + '</font>', function (btn, text) {
                        Ext.MessageBox.confirm('寄送匯款通知單', '是否確定寄送匯款通知單？', function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var remitNos = '';                            
                            T1Store.each(function (record) {
                                var remitNo = record.get('REMITNO');
                                remitNos += remitNo + ',';
                            });

                            Ext.Ajax.request({
                                url: sendMail,
                                method: reqVal_p,
                                params: {
                                    p0: T1Query.getForm().findField('P0').rawValue,
                                    p1: remitNos
                                },
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.MessageBox.alert('訊息', '信件寄送完成');
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {
                                    myMask.hide();
                                    Ext.MessageBox.alert('錯誤', '發生非預期的錯誤.');
                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                itemId: 'reportForm', text: '委託電匯資料表', disabled: true,
                handler: function () {
                    popReportFormWin(T1Query.getForm().findField('P0').rawValue);
                }
            }
        ]
    });
    function setFormT1(x, t) {
        //viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        u = f.findField('XFRMEMO');
        f.findField('x').setValue(x);
        f.findField('AGEN_BANK_14').setReadOnly(false);
        f.findField('AGEN_ACC').setReadOnly(false);
        f.findField('PO_AMT').setReadOnly(false);
        f.findField('ADDORSUB_AMT').setReadOnly(false);
        f.findField('DISC_AMT').setReadOnly(false);
        f.findField('PROCFEE').setReadOnly(false);
        f.findField('MGTFEE').setReadOnly(false);
        f.findField('XFRMEMO').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 50,
        }, {
            text: '已匯出',
            width: 64,
            stopSelection: false,
            xtype: 'templatecolumn',
            tpl: ['<tpl for="."><input type="checkbox" disabled="disabled" </tpl>',
                '<tpl if="ISREMIT == \'B\'">checked="checked"</tpl>',
                '  />'
            ],
            menuDisabled: true
        }, {
            text: "匯款單編號",
            dataIndex: 'REMITNO',
            width: 100,
        }, {
            text: "匯款日期",
            dataIndex: 'REMITDATE',
            width: 130
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            width: 100,
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAME',
            width: 300,
        }, {
            text: "匯款總金額",
            dataIndex: 'REMIT',
            width: 300,
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                if (records.length) {
                    setFormT1("U", '修改');
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    //if (T1LastRec.data.AGEN_BANK_14.length == 0 || T1LastRec.data.AGEN_ACC.length == 0)
                    //    Ext.Msg.alert('訊息', '此廠商尚未維護銀行代碼或匯款帳號');
                    setFormT1a();
                    viewport.down('#form').expand();
                }
                else
                    T1Cleanup();
            }
        }
    });
    function setFormT1a() {
        //T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#reloadAgenBank').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1Grid.down('#export').setDisabled(T1LastRec.data.ISREMIT == 'A');
            T1Grid.down('#sendMail').setDisabled(T1LastRec.data.ISREMIT == 'A');
            //若重新載入鈕要限制已匯出資料不可使用則改用下面這行
            //T1Grid.down('#reloadAgenBank').setDisabled(T1LastRec.data.ISREMIT != 'A');
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
        }
        else {
            T1Grid.down('#export').setDisabled(true);
            T1Grid.down('#sendMail').setDisabled(true);
            T1Form.getForm().reset();
        }
    }

    var ChkResultWin = null;
    popChkResultWin = function (errym, errmsg) {
        var ChkResultFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelAlign: 'right',
            },
            items: [{
                xtype: 'panel',
                id: 'PanelChkResult',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '月結月份',
                        name: 'ChkYM',
                        width: '100%'
                    }, {
                        xtype: 'textareafield',
                        fieldLabel: '明細',
                        name: 'ChkResult',
                        width: '100%',
                        height: 200,
                        readOnly: true
                    }
                ]
            }]
        });

        var ChkResultForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [ChkResultFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '下載Txt檔',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: errym });
                    PostForm(T1GetChkTxt, p);
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    ChkResultWin.destroy();
                    ChkResultWin = null;
                }
            }]
        });
        ChkResultWin = GetPopWin(viewport, ChkResultForm, '電匯資料檢查結果', 650, 350);

        ChkResultWin.show();

        ChkResultFormContent.getForm().findField('ChkYM').setValue(errym);
        ChkResultFormContent.getForm().findField('ChkResult').setValue(errmsg);
    }

    // 轉電匯資料查詢視窗
    var DownloadFormWin = null;
    popDownloadFormWin = function (dataym) {
        var st_remitno = Ext.create('Ext.data.Store', {
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/HA0001/GetRemitnoCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            },
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件代入參數
                    var np = {
                        p0: dataym
                    };
                    Ext.apply(store.proxy.extraParams, np);
                },
                load: function (store, records, successful, eOpts) {
                    if (successful) {
                        if (records.length > 0) {
                            DownloadFormContent.getForm().findField('RemitnoFrom').setValue(records[0].data.VALUE);
                            DownloadFormContent.getForm().findField('RemitnoTo').setValue(records[records.length - 1].data.VALUE);
                        }
                    }
                }
            }
        });

        var DownloadFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 130,
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelDownloadForm',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'datefield',
                        fieldLabel: '電匯日期',
                        name: 'DownloadDate',
                        width: '100%',
                        fieldCls: 'required',
                        allowBlank: false,
                        padding: '0 4 0 4',
                        value: new Date()
                    }, {
                        xtype: 'combo',
                        fieldLabel: '轉電匯編號區間',
                        name: 'RemitnoFrom',
                        width: '100%',
                        store: st_remitno,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '至',
                        name: 'RemitnoTo',
                        width: '100%',
                        store: st_remitno,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '匯款人帳戶',
                        name: 'HospBankAcc',
                        padding: '0 4 0 4',
                        width: '100%',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '匯款人姓名',
                        name: 'HospName',
                        padding: '0 4 0 4',
                        width: '100%',
                        readOnly: true
                    }
                ]
            }]
        });

        var DownloadForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [DownloadFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '下載',
                handler: function () {
                    Ext.MessageBox.confirm('載入', '是否確定下載' + dataym + '電匯資料？', function (btn, text) {
                        if (btn === 'yes') {
                            var P1 = DownloadFormContent.getForm().findField('DownloadDate').rawValue;
                            var P2 = DownloadFormContent.getForm().findField('RemitnoFrom').getValue();
                            var P3 = DownloadFormContent.getForm().findField('RemitnoTo').getValue();
                            Ext.Ajax.request({
                                // 檢查下載範圍是否有已下載過的部分
                                url: isremitChk,
                                method: reqVal_p,
                                params: {
                                    p0: dataym,
                                    p2: P2,
                                    p3: P3
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        var p = new Array();
                                        p.push({ name: 'p0', value: dataym });
                                        p.push({ name: 'p1', value: P1 });
                                        p.push({ name: 'p2', value: P2 });
                                        p.push({ name: 'p3', value: P3 });
                                        if (data.msg == 'P') { // 無已下載過的單子
                                            // 檢查下載範圍是否有銀行代碼無資料的廠商
                                            Ext.Ajax.request({
                                                url: bankChk,
                                                method: reqVal_p,
                                                params: {
                                                    p0: dataym,
                                                    p2: P2,
                                                    p3: P3
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    if (data.success) {
                                                        if (data.msg == 'P') {
                                                            PostForm(T1GetTxt, p);
                                                        }
                                                        else {
                                                            Ext.MessageBox.confirm('訊息', '匯款單編號包含無銀行代碼或帳戶的廠商,是否匯出且不包含這些匯款單？', function (btn, text) {
                                                                if (btn === 'yes') {
                                                                    PostForm(T1GetTxt, p);
                                                                }
                                                            }
                                                            );
                                                        }
                                                    } else {
                                                        Ext.MessageBox.alert('錯誤', data.msg);
                                                    }
                                                },
                                                failure: function (response, options) {

                                                }
                                            });
                                        }
                                        else {  // 有已下載過的單子
                                            Ext.MessageBox.confirm('訊息', '匯款單編號包含已匯出過的資料,是否重新匯出並更新電匯日期？', function (btn, text) {
                                                if (btn === 'yes') {
                                                    // 檢查下載範圍是否有銀行代碼無資料的廠商
                                                    Ext.Ajax.request({
                                                        url: bankChk,
                                                        method: reqVal_p,
                                                        params: {
                                                            p0: dataym,
                                                            p2: P2,
                                                            p3: P3
                                                        },
                                                        success: function (response) {
                                                            var data = Ext.decode(response.responseText);
                                                            if (data.success) {
                                                                if (data.msg == 'P') {
                                                                    PostForm(T1GetTxt, p);
                                                                }
                                                                else {
                                                                    Ext.MessageBox.confirm('訊息', '匯款單編號包含無銀行代碼或帳戶的廠商,是否匯出且不包含這些匯款單？', function (btn, text) {
                                                                        if (btn === 'yes') {
                                                                            PostForm(T1GetTxt, p);
                                                                        }
                                                                    }
                                                                    );
                                                                }
                                                            } else {
                                                                Ext.MessageBox.alert('錯誤', data.msg);
                                                            }
                                                        },
                                                        failure: function (response, options) {

                                                        }
                                                    });
                                                }
                                            }
                                            );
                                        }
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {

                                }
                            });
                        }
                    }
                    );
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    DownloadFormWin.destroy();
                    DownloadFormWin = null;
                    T1Load();
                }
            }]
        });
        var formWinTitle = dataym.substring(0, 3) + '年' + dataym.substring(3, 5) + '月份轉電匯資料';
        DownloadFormWin = GetPopWin(viewport, DownloadForm, formWinTitle, 300, 300);

        DownloadFormWin.show();

        // 載入電匯編號選單
        st_remitno.load();
        // 載入匯款人帳戶,姓名
        Ext.Ajax.request({
            url: hospbankGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnVal = data.msg.split('^');
                    DownloadFormContent.getForm().findField('HospBankAcc').setValue(rtnVal[0]);
                    DownloadFormContent.getForm().findField('HospName').setValue(rtnVal[1]);
                } else {
                    Ext.MessageBox.alert('錯誤', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 列印委託電匯資料表查詢視窗
    var ReportFormWin = null;
    popReportFormWin = function (dataym) {
        var st_remitno = Ext.create('Ext.data.Store', {
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/HA0001/GetRemitnoCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            },
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件代入參數
                    var np = {
                        p0: dataym
                    };
                    Ext.apply(store.proxy.extraParams, np);
                },
                load: function (store, records, successful, eOpts) {
                    if (successful) {
                        if (records.length > 0) {
                            ReportFormContent.getForm().findField('RemitnoFrom').setValue(records[0].data.VALUE);
                            ReportFormContent.getForm().findField('RemitnoTo').setValue(records[records.length - 1].data.VALUE);
                        }
                    }
                }
            }
        });

        var ReportFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 130,
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelReportForm',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '列印',
                        name: 'PrintType',
                        width: '100%',
                        store: PRINT_TYPE_store,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'DESC',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '匯款單編號區間',
                        name: 'RemitnoFrom',
                        width: '100%',
                        store: st_remitno,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '至',
                        name: 'RemitnoTo',
                        width: '100%',
                        store: st_remitno,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }
                ]
            }]
        });

        var ReportForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [ReportFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '列印',
                handler: function () {
                    var P1 = ReportFormContent.getForm().findField('PrintType').getValue();
                    var P2 = ReportFormContent.getForm().findField('RemitnoFrom').getValue();
                    var P3 = ReportFormContent.getForm().findField('RemitnoTo').getValue();

                    ReportFormWin.destroy();
                    ReportFormWin = null;

                    showReport(P1, P2, P3);
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    ReportFormWin.destroy();
                    ReportFormWin = null;
                    // T1Load();
                }
            }]
        });
        var formWinTitle = '列印' + dataym.substring(0, 3) + '年' + dataym.substring(3, 5) + '月份委託電匯資料表';
        ReportFormWin = GetPopWin(viewport, ReportForm, formWinTitle, 300, 200);

        ReportFormWin.show();

        // 載入電匯編號選單
        st_remitno.load();
        ReportFormContent.getForm().findField('PrintType').setValue('0');
    }

    function showReport(printType, remitnoFrom, remitnoTo) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?printType=' + printType + '&remitnoFrom=' + remitnoFrom + '&remitnoTo=' + remitnoTo + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;

        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;


        var mm = m >= 10 ? m.toString() : "0" + m.toString();

        return yyyy.toString() + mm;

    }

    function calAMT() {
        // 重算應付金額
        var newAMTPAYABLE = parseFloat(T1Form.getForm().findField('PO_AMT').getValue())
            + parseFloat(T1Form.getForm().findField('ADDORSUB_AMT').getValue());
        T1Form.getForm().findField('AMTPAYABLE').setValue(newAMTPAYABLE);

        // 重算實付金額
        var newAMTPAID = parseFloat(T1Form.getForm().findField('AMTPAYABLE').getValue())
            - parseFloat(T1Form.getForm().findField('DISC_AMT').getValue());
        T1Form.getForm().findField('AMTPAID').setValue(newAMTPAID);

        // 重算匯款總金額
        var newREMIT = parseFloat(T1Form.getForm().findField('AMTPAID').getValue())
            - parseFloat(T1Form.getForm().findField('PROCFEE').getValue())
            - parseFloat(T1Form.getForm().findField('MGTFEE').getValue());
        T1Form.getForm().findField('REMIT').setValue(newREMIT);
    }

    function calAmtMsg() {
        // 統計總匯款量
        Ext.Ajax.request({
            url: amtGet,
            method: reqVal_p,
            params: {
                DATA_YM: T1Query.getForm().findField('P0').rawValue,
                AGEN: T1Query.getForm().findField('P1').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '') {
                        T1Query.down('#T1Query_AmtMsg').setText('總匯款量：' + data.msg);
                    }
                } else {
                    T1Query.down('#T1Query_AmtMsg').setText('總匯款量：');
                }
            },
            failure: function (response, options) {
            }
        });

    }

    function queryChk() {
        Ext.Ajax.request({
            url: chkQuery,
            method: reqVal_p,
            params: {
                p0: T1Query.getForm().findField('P0').rawValue,
                p1: T1Query.getForm().findField('P1').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {

                    }
                    else {
                        // 檢查不通過時顯示廠商清單
                        var chkYM = '';
                        var showMsg = '';
                        if (data.etts) {
                            var records = data.etts;
                            chkYM = records[0]['DATA_YM'];
                            for (var i = 0; i < records.length; i++) {
                                showMsg = showMsg + '廠商代碼:' + records[i]['AGEN_NO'] + ' ' + records[i]['CHKMSG'] + '\n';
                            }
                            popChkResultWin(chkYM, showMsg);
                        }
                    }
                    T1Load();
                } else {
                    Ext.MessageBox.alert('錯誤', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

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
            split: true,
            width: '100%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'south',
            collapsible: true,
            floatable: true,
            split: true,
            // width: '20%',
            minWidth: 120,
            minHeight: 140,
            title: '瀏覽',
            collapsed: true,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    getInitConfig();

    T1Query.getForm().findField('P0').focus();
    msglabel('');
});