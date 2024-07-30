Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },
            { name: 'CR_D_SEQ', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'INQTY', type: 'string' },
            { name: 'ISUDI', type: 'string' }
        ]
    });

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();


    var T1Set = '';
    var T1LastRec = null;
    var maxExpDate = '';

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'StartDate', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0110/Details',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    crdocno: CrDocForm.getForm().findField('CRDOCNO').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    var T2Store = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function T1Load() {
        T1Tool.moveFirst();
        
    }

    // 查詢欄位
    var mLabelWidth = 135;
    var mWidth = 315;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '刷緊急醫療出貨單編號',
                                name: 'CRDOCNO',
                                id: 'CRDOCNO',
                                listeners: {
                                    change: function (field, nValue, oValue, eOpts) {
                                        if (nValue.length > 9) {
                                            
                                            //chkBarcode(nValue);
                                            CheckCrDoc(T1Query.getForm().findField('CRDOCNO').getValue());
                                        }
                                    }
                                }
                            }
                            , {
                                xtype: 'button',
                                text: '顯示',
                                name:'showInfo',
                                handler: function () {
                                    msglabel('訊息區:');

                                    CheckCrDoc(T1Query.getForm().findField('CRDOCNO').getValue());
                                }
                            },
                            {
                                xtype: 'button',
                                text: '查詢緊急醫療訂單資料',
                                handler: function () {
                                    popWinForm('AB0111');
                                }
                            },
                        ]
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'button',
                                text: '修改院內碼',
                                handler: function () {
                                    mmcodeForm.getForm().findField('ACKMMCODE').setValue(CrDocForm.getForm().findField('ACKMMCODE').getValue());
                                    mmcodeForm.getForm().findField('MMNAME_C').setValue(CrDocForm.getForm().findField('MMNAME_C').getValue());
                                    mmcodeForm.getForm().findField('MMNAME_E').setValue(CrDocForm.getForm().findField('MMNAME_E').getValue());
                                    mmcodeForm.getForm().findField('BASE_UNIT').setValue(CrDocForm.getForm().findField('BASE_UNIT').getValue());
                                    mmcodeForm.getForm().findField('ISSMALL_ORI').setValue(CrDocForm.getForm().findField('ISSMALL').getValue());
                                    
                                    mmcodeForm.getForm().findField('USEWHEN').setValue(CrDocForm.getForm().findField('USEWHEN').getValue());
                                    mmcodeForm.getForm().findField('USEWHERE').setValue(CrDocForm.getForm().findField('USEWHERE').getValue());
                                    mmcodeForm.getForm().findField('TEL').setValue(CrDocForm.getForm().findField('TEL').getValue());

                                    mmcodeWindow.show();
                                }
                            },
                            {
                                xtype: 'button',
                                text: '點收',
                                handler: function () {
                                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                    myMask.show();

                                    Ext.Ajax.request({
                                        url: '/api/AB0110/CheckQtyValid',
                                        method: reqVal_p,
                                        params: {
                                            crdocno: CrDocForm.getForm().findField('CRDOCNO').getValue()
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            myMask.hide();
                                            if (data.success) {
                                                if (data.msg == '0') {
                                                    Ext.Msg.alert('提醒', '點收量不可為0');
                                                    MasterLoad(CrDocForm.getForm().findField('CRDOCNO').getValue());
                                                }else if (data.msg == '<') {
                                                    Ext.Msg.alert('提醒', '點收量 > 申請量，不可點收');
                                                    MasterLoad(CrDocForm.getForm().findField('CRDOCNO').getValue());
                                                }else  if (data.msg == '>') {
                                                    Ext.MessageBox.confirm('提示', '點收量<申請量，是否點收？', function (btn, text) {
                                                        if (btn === 'yes') {
                                                            confirm(CrDocForm.getForm().findField('CRDOCNO').getValue());
                                                        }
                                                    });
                                                } else if (data.msg == '=') {
                                                    confirm(CrDocForm.getForm().findField('CRDOCNO').getValue());
                                                }
                                            } else {
                                                Ext.Msg.alert('提醒', data.msg);
                                            }
                                        },
                                        failure: function (response, options) {
                                            Ext.Msg.alert('失敗', '發生例外錯誤');
                                        }
                                    });
                                }
                            },
                            {
                                xtype: 'button',
                                text: '退回',
                                handler: function () {
                                    Ext.MessageBox.confirm('提示', '是否確定退回？', function (btn, text) {
                                        if (btn === 'yes') {
                                            reject(CrDocForm.getForm().findField('CRDOCNO').getValue());
                                        }
                                    });
                                }
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '<span style="color:red">※提示</span>',
                                value: '',
                                width:380,
                                renderer: function () {
                                    return '<span style="color:red">若需修改院內碼，請先修改再新增批號效期</span>'
                                }
                            },
                        ]
                    }
                ]
            },
        ]
    });
    var CrDocForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                items:
                [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '緊急醫療出貨單編號',
                        name: 'CRDOCNO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收院內碼',
                        name: 'ACKMMCODE',
                        labelAlign: 'right',
                        colspan: 2
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        labelAlign: 'right',
                        colspan: 3,
                        width: mWidth*3
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        labelAlign: 'right',
                        colspan: 3,
                        width: mWidth * 3
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請量',
                        name: 'APPQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '計量單位',
                        name: 'BASE_UNIT',
                        labelAlign: 'right',
                        colspan: 2
                    },
                    
                    {
                        xtype: 'displayfield',
                        fieldLabel: '入庫庫房名稱',
                        name: 'WH_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'CR_STATUS_NAME',
                        labelAlign: 'right',
                        colspan: 2
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '進貨量',
                        name: 'INQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商回覆批號',
                        name: 'LOT_NO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商回覆效期',
                        name: 'EXP_DATE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '用途',
                        name: 'USEWHERE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '本次申請量預估使用時間',
                        name: 'USEWHEN',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '電話',
                        name: 'TEL',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'CR_STATUS',
                        labelAlign: 'right',
                        hidden:true
                    },
                    {
                        xtype: 'hidden',
                        name: 'ISSMALL',
                    },
                ]
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
                itemId: 'scanInsert', text: '刷條碼新增', disabled: true,
                id: 'scanInsert',
                handler: function () {
                    T2Query.getForm().findField('CRDOCNO').setValue(CrDocForm.getForm().findField('CRDOCNO').getValue());
                    T2Query.getForm().findField('ACKMMCODE').setValue(CrDocForm.getForm().findField('ACKMMCODE').getValue());

                    scanWindow.show();

                    //setFormT1('U', '修改');
                }
            },
            {
                itemId: 'insert', text: '新增', disabled: true,
                id: 'insert',
                handler: function () {
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'update', text: '修改', disabled: true,
                id: 'update',
                handler: function () {
                    setFormT1('U', '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                id: 'delete',
                handler: function () {
                    Ext.MessageBox.confirm('提示', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'importData', text: '載入批號效期', disabled: true,
                id: 'importData',
                handler: function () {
                    importData(CrDocForm.getForm().findField('CRDOCNO').getValue());
                }
            }
        ]
    });

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
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [CrDocForm]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "點收量",
                dataIndex: 'INQTY',
                width: 110,
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 110,
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 130,
            },
            {
                text: "刷UDI",
                dataIndex: 'ISUDI',
                width: 80
            },
            
            {
                header: "",
                flex: 1
            }
        ]
        ,
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');

                T1LastRec = record;

                Ext.getCmp('update').enable();
                Ext.getCmp('delete').enable();

                setFormT1a();

            }
        }
    });

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
                fieldLabel: 'CR_D_SEQ',
                name: 'CR_D_SEQ',
                xtype: 'hidden'
            },
            {
                fieldLabel: '緊急醫療出貨單編號',
                name: 'CRDOCNO',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '院內碼',
                name: 'ACKMMCODE',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                xtype: 'displayfield'
            },
            {
                xtype: 'numberfield',
                fieldLabel: '點收量',
                name: 'INQTY',
                maskRe: /[0-9]/,
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true,
                allowBlank: false,
                fieldCls: 'required',
                hideTrigger: true,
                minValue: 1,
                decimalPrecision: 0
            },
            //{
            //    fieldLabel: '借貨量',
            //    name: 'BW_SQTY',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    submitValue: true,
            //    readOnly: true
            //},
            {
                fieldLabel: '批號',
                name: 'LOT_NO',
                enforceMaxLength: true,
                maxLength: 20,
                submitValue: true,
                readOnly: true,
                allowBlank: false,
                fieldCls: 'required',
                //allowBlank: false // [交貨數量]=0資料可以不輸入批號、效期。

            },
            {
                xtype: 'datefield',
                fieldLabel: '效期',
                name: 'EXP_DATE',
                regex: /\d{7,7}/,
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true,
                allowBlank: false,
                fieldCls: 'required',
                //allowBlank: false // 欄位為必填


            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    var f = T1Form.getForm();
                    var msg = "";
                    var LOT_NO = f.findField("LOT_NO").getValue();
                    var EXP_DATE = f.findField("EXP_DATE").getValue();
                    var INQTY = f.findField("INQTY").getValue();

                    if (INQTY <= 0) {
                        msg += "點收量 不可為0<br>";
                    }
                    //if (INQTY > Number(CrDocForm.getForm().findField('INQTY').getValue())) {
                    //    msg += "點收量不可超過進貨量";
                    //}

                  
                    if (LOT_NO == "" || EXP_DATE == null) {
                        msg += "效期、批號 不可以空白<br>";
                    }


                    if (msg != "") {
                        Ext.Msg.alert('訊息提示', msg);
                        return;
                    }
                    
                    //if (new Date(EXP_DATE) > new Date(maxExpDate)) {
                    //    msg = '效期超過最大值，已更新為最大效期，請確認';
                    //    f.findField("EXP_DATE").setValue(new Date(maxExpDate));
                    //    Ext.Msg.alert('訊息提示', msg);
                    //    return;
                    //}

                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
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

            var url = '/api/AB0110/';
            var temp = '';
            if (f.findField('x').getValue() == 'I') {
                url += 'InsertSingle';
                temp = '新增';
            }
            if (f.findField('x').getValue() == 'U') {
                url += 'Update';
                temp = '修改';
            }
            if (f.findField('x').getValue() == 'D') {
                url += 'Delete';
                temp = '刪除';
            }
            Ext.Ajax.request({
                url: url,
                method: reqVal_p,
                params: {
                    crdocno: f.findField('CRDOCNO').getValue(),
                    cr_d_seq: f.findField('CR_D_SEQ').getValue(),
                    inqty: f.findField('INQTY').getValue(),
                    lot_no: f.findField('LOT_NO').getValue(),
                    exp_date: f.findField('EXP_DATE').rawValue,
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        myMask.hide();
                        viewport.down('#form').collapse();


                        msglabel(temp + "成功");
                        
                        T1Query.getForm().findField('CRDOCNO').setValue(T1Form.getForm().findField('CRDOCNO').getValue());
                        T1Load();
                        T1Cleanup();

                    } else {
                        Ext.Msg.alert('提醒', data.msg);
                    }
                },
                failure: function (response, options) {
                    Ext.Msg.alert('失敗', '發生例外錯誤');
                }
            });

        }
    }

    function T1Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();

        var f = T1Form.getForm();

        f.findField('x').setValue(x);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('INQTY').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);

        if (x === "I") {

            T1Form.getForm().findField('CRDOCNO').setValue(CrDocForm.getForm().findField('CRDOCNO').getValue());
            T1Form.getForm().findField('ACKMMCODE').setValue(CrDocForm.getForm().findField('ACKMMCODE').getValue());
            T1Form.getForm().findField('MMNAME_C').setValue(CrDocForm.getForm().findField('MMNAME_C').getValue());
            T1Form.getForm().findField('MMNAME_E').setValue(CrDocForm.getForm().findField('MMNAME_E').getValue());

            T1Form.getForm().findField('INQTY').setValue('');
            T1Form.getForm().findField('LOT_NO').setValue('');
            T1Form.getForm().findField('EXP_DATE').setValue('');
        } 
        //u.focus();
    }

    function setFormT1a() {

        if (T1LastRec != null) {
            viewport.down('#form').expand();
            viewport.down('#form').setTitle('瀏覽');

            T1Form.loadRecord(T1LastRec);

            T1Form.getForm().findField('CRDOCNO').setValue(CrDocForm.getForm().findField('CRDOCNO').getValue());
            T1Form.getForm().findField('ACKMMCODE').setValue(CrDocForm.getForm().findField('ACKMMCODE').getValue());
            T1Form.getForm().findField('MMNAME_C').setValue(CrDocForm.getForm().findField('MMNAME_C').getValue());
            T1Form.getForm().findField('MMNAME_E').setValue(CrDocForm.getForm().findField('MMNAME_E').getValue());
            T1Form.getForm().findField('USEWHEN').setValue(CrDocForm.getForm().findField('USEWHEN').getValue());
            T1Form.getForm().findField('USEWHERE').setValue(CrDocForm.getForm().findField('USEWHERE').getValue());
            T1Form.getForm().findField('TEL').setValue(CrDocForm.getForm().findField('TEL').getValue());



            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    function CheckCrDoc(crdocno) {
        var f = CrDocForm.getForm();
        f.findField('CRDOCNO').setValue('');
        f.findField('ACKMMCODE').setValue('');
        f.findField('MMNAME_C').setValue('');
        f.findField('MMNAME_E').setValue('');
        f.findField('APPQTY').setValue('');
        f.findField('BASE_UNIT').setValue('');
        f.findField('INQTY').setValue('');
        f.findField('WH_NAME').setValue('');
        f.findField('CR_STATUS_NAME').setValue('');
        f.findField('CR_STATUS').setValue('');
        f.findField('LOT_NO').setValue('');
        f.findField('EXP_DATE').setValue('');
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/CheckCrDoc',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    MasterLoad(crdocno);

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function MasterLoad(crdocno) {
        Ext.Ajax.request({
            url: '/api/AB0110/Master',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts[0];
                    
                    var f = CrDocForm.getForm();
                    f.findField('CRDOCNO').setValue(temp.CRDOCNO);
                    f.findField('ACKMMCODE').setValue(temp.ACKMMCODE);
                    f.findField('MMNAME_C').setValue(temp.MMNAME_C);
                    f.findField('MMNAME_E').setValue(temp.MMNAME_E);
                    f.findField('APPQTY').setValue(temp.APPQTY);
                    f.findField('BASE_UNIT').setValue(temp.BASE_UNIT);
                    f.findField('INQTY').setValue(temp.INQTY);
                    f.findField('WH_NAME').setValue(temp.WH_NAME);
                    f.findField('CR_STATUS_NAME').setValue(temp.CR_STATUS_NAME);
                    f.findField('CR_STATUS').setValue(temp.CR_STATUS);
                    f.findField('LOT_NO').setValue(temp.LOT_NO);
                    f.findField('EXP_DATE').setValue(temp.EXP_DATE);
                    f.findField('USEWHEN').setValue(temp.USEWHEN);
                    f.findField('USEWHERE').setValue(temp.USEWHERE);
                    f.findField('TEL').setValue(temp.TEL);

                    Ext.getCmp('scanInsert').enable();
                    Ext.getCmp('insert').enable();
                    Ext.getCmp('importData').enable();
                    
                    Ext.getCmp('update').disable();
                    Ext.getCmp('delete').disable();

                    viewport.down('#form').collapse();

                    T1Load();
                } else {
                    
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    //#region scanWindow
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '緊急醫療出貨單編號',
                                name: 'CRDOCNO',
                            },
                            {
                                xtype: 'displayfield',
                                fieldLabel: '點收院內碼',
                                name: 'ACKMMCODE',
                            }
                        ]
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '刷條碼',
                                name: 'Barcode',
                                listeners: {
                                    change: function (field, nValue, oValue, eOpts) {
                                        if (nValue.length > 9) {
                                            
                                            //chkBarcode(nValue);
                                            //CheckCrDoc(T1Query.getForm().findField('CRDOCNO').getValue());
                                            getScanData(T2Query.getForm().findField('Barcode').getValue(),
                                                T2Query.getForm().findField('CRDOCNO').getValue(),
                                                T2Query.getForm().findField('ACKMMCODE').getValue());
                                        }
                                    }
                                }
                            }
                        ]
                    }
                ]
            },
        ]
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'ACKMMCODE',
                width: 110,
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 110,
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 110,
            },
            {
                text: "點收量",
                dataIndex: 'INQTY',
                width: 110,
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 110,
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 130,
            },
            {
                text: "刷UDI",
                dataIndex: 'ISUDI',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ]
        ,
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');

                T1LastRec = record;

                Ext.getCmp('scanInsert').enable();
                Ext.getCmp('insert').enable();
                Ext.getCmp('importData').enable();
                Ext.getCmp('update').enable();
                Ext.getCmp('delete').enable();

                //if (record.data.IN_STATUS == 'A') {
                //    Ext.getCmp('update').enable();
                //    Ext.getCmp('confirm').enable();
                //} else if (record.data.IN_STATUS == 'B') {
                //    Ext.getCmp('print').enable();

                //} else if (record.data.IN_STATUS == 'C') {
                //    Ext.getCmp('print').enable();
                //}

                setFormT1a();

            }
        }
    });

    function getScanData(barcode, crdocno, ackmmcode) {
        var myMask = new Ext.LoadMask(scanWindow, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/CheckUdiData',
            method: reqVal_p,
            params: {
                crdocno: crdocno,
                barcode: barcode,
                ackmmcode: ackmmcode
            },
            success: function (response) {
                myMask.hide();
                
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    var tb_data = data.etts;
                    if (tb_data) {
                        if (tb_data.length > 0) {
                            UdiRec = tb_data[0];
                            T2Store.add(UdiRec);
                            T2Query.getForm().findField('Barcode').setValue('');
                            T2Query.getForm().findField('Barcode').focus();
                        }
                        else
                            chkBarcode(barcode, crdocno, ackmmcode);
                    }
                    else
                        chkBarcode(barcode, crdocno, ackmmcode);
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }
    function chkBarcode(barcode, crdocno, ackmmcode) {
        var myMask = new Ext.LoadMask(scanWindow, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/CheckBcBarcode',
            method: reqVal_p,
            params: {
                crdocno: crdocno,
                barcode: barcode,
                ackmmcode: ackmmcode
            },
            success: function (response) {
                myMask.hide();

                var data = Ext.decode(response.responseText);
                if (data.success) {

                    var temp = data.etts[0];
                    T2Store.add(temp);

                    T2Query.getForm().findField('Barcode').setValue('');
                    T2Query.getForm().findField('Barcode').focus();
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }
    function insertMulti() {
        var myMask = new Ext.LoadMask(scanWindow, { msg: '處理中...' });
        myMask.show();

        
        var data = [];
        var tempData = T2Grid.getStore().data.items;
        for (var i = 0; i < tempData.length; i++) {
            var temp = {
                CRDOCNO: T2Query.getForm().findField('CRDOCNO').getValue(),
                ACKMMCODE: tempData[i].data.ACKMMCODE,
                INQTY: tempData[i].data.INQTY,
                ISUDI: tempData[i].data.ISUDI,
                LOT_NO: tempData[i].data.LOT_NO,
                EXP_DATE: tempData[i].data.EXP_DATE,
                EXP_DATE: tempData[i].data.EXP_DATE,
            };
            data.push(temp);
        }
        Ext.Ajax.request({
            url: '/api/AB0110/InsertMulti',
            method: reqVal_p,
            params: {
                list: Ext.util.JSON.encode(data),
            },
            success: function (response) {
                myMask.hide();

                var data = Ext.decode(response.responseText);
                if (data.success) {

                    msglabel('刷條碼新增成功');
                    scanWindow.hide();

                    T1Load();
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var scanWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T2Grid],
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "刷條碼新增",
        buttons: [
            {
                text: '完成',
                handler: function () {
                    //checkSetRatioEmpty();
                    
                    insertMulti();
                    //scanWindow.hide();
                }
            },
            {
                text: '關閉',
                handler: function () {
                    //checkSetRatioEmpty();
                    //getLastUploadtTime();
                    //T2Query.getForm().findField('starOnly').setValue(false);
                    scanWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                scanWindow.center();
                T2Query.getForm().findField('Barcode').setValue('');
                T2Query.getForm().findField('Barcode').focus();
            }
        }
    });
    scanWindow.hide();
    //#endregion

    //#region mmcodeWindow
    var mmcodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'ACKMMCODE',
        fieldLabel: '點收院內碼',
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0110/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'ISSMALL'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
          
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                mmcodeForm.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                mmcodeForm.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                mmcodeForm.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
                mmcodeForm.getForm().findField('ISSMALL').setValue(r.get('ISSMALL'));
            }
        }
    });

    var mmcodeForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            mmcodeCombo,
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
            },
            {
                xtype: 'hidden',
                name: 'ISSMALL_ORI',
            },
            {
                xtype: 'hidden',
                name: 'ISSMALL',
            },
            {
                xtype: 'textfield',
                fieldLabel: '用途',
                name: 'USEWHERE',
            },
            {
                xtype: 'textfield',
                fieldLabel: '本次申請量<br>預估使用時間',
                name: 'USEWHEN',
            },
            {
                xtype: 'textfield',
                fieldLabel: '電話',
                name: 'TEL',
            },
        ]
    });

    function updateAckmmcode(crdocno, ackmmcode, usewhere, usewhen, tel) {
        var myMask = new Ext.LoadMask(mmcodeWindow, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/ChangeMmcode',
            method: reqVal_p,
            params: {
                crdocno: crdocno,
                ackmmcode: ackmmcode,
                usewhere: usewhere,
                usewhen: usewhen,
                tel: tel
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    mmcodeWindow.hide();
                    msglabel('院內碼更新成功');
                    MasterLoad(crdocno);
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var mmcodeWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [mmcodeForm],
        width: 300,
        height: 300,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "修改院內碼",
        buttons: [
            {
                text: '儲存',
                handler: function () {

                    if (!mmcodeForm.getForm().findField('ACKMMCODE').getValue()) {
                        Ext.Msg.alert('提醒', '院內碼為必填');
                        return;
                    }
                    if (mmcodeForm.getForm().findField('ISSMALL').getValue() == 'Y' &&
                        (!mmcodeForm.getForm().findField('USEWHERE').getValue() ||
                        !mmcodeForm.getForm().findField('USEWHEN').getValue() ||
                        !mmcodeForm.getForm().findField('TEL').getValue()
                        ) ) {
                        Ext.Msg.alert('提醒', '小採院內碼 用途、使用時間、電話 為必填');
                        return;
                    }

                    updateAckmmcode(CrDocForm.getForm().findField('CRDOCNO').getValue(),
                        mmcodeForm.getForm().findField('ACKMMCODE').getValue(),
                        mmcodeForm.getForm().findField('USEWHERE').getValue(),
                        mmcodeForm.getForm().findField('USEWHEN').getValue(),
                        mmcodeForm.getForm().findField('TEL').getValue());


                    //checkSetRatioEmpty();
                    //mmcodeWindow.hide();
                    //insertMulti();
                    //scanWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    //checkSetRatioEmpty();
                    //getLastUploadtTime();
                    //T2Query.getForm().findField('starOnly').setValue(false);
                    mmcodeWindow.hide();
                }
            }
        ]
    });
    mmcodeWindow.hide();
    //#endregion

    //#region confirmWindow
    var confirmForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '<span style="color:red;font-weight:bold">※提示</span>',
                value: '',
                width: 380,
                renderer: function () {
                    return '<span style="color:red;font-weight:bold">點收後，應完成紙本用印並雙方收執</span>'
                }
            },
        ]
    });


    var confirmWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [confirmForm],
        //width: 300,
        //height: 300,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "提醒",
        buttons: [
            {
                text: '確認',
                handler: function () {
                    confirmWindow.hide();

                    T1Query.getForm().findField('CRDOCNO').setValue('');

                    var f = CrDocForm.getForm();
                    f.findField('CRDOCNO').setValue('');
                    f.findField('ACKMMCODE').setValue('');
                    f.findField('MMNAME_C').setValue('');
                    f.findField('MMNAME_E').setValue('');
                    f.findField('APPQTY').setValue('');
                    f.findField('BASE_UNIT').setValue('');
                    f.findField('INQTY').setValue('');
                    f.findField('WH_NAME').setValue('');
                    f.findField('CR_STATUS_NAME').setValue('');
                    f.findField('CR_STATUS').setValue('');
                    f.findField('LOT_NO').setValue('');
                    f.findField('EXP_DATE').setValue('');

                    Ext.getCmp('scanInsert').disable();
                    Ext.getCmp('insert').disable();
                    Ext.getCmp('importData').disable();
                    Ext.getCmp('update').disable();
                    Ext.getCmp('delete').disable();

                    T1Store.removeAll();

                    T1Query.getForm().findField('CRDOCNO').focus();
                }
            }
        ]
    });
    confirmWindow.hide();

    //#endregion

    function confirm(crdocno) {

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Confirm',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("點收成功");

                    confirmWindow.show();

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);

                    T1Query.getForm().findField('CRDOCNO').setValue(crdocno);
                    MasterLoad(crdocno);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function reject(crdocno) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Reject',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("退回成功");

                    T1Query.getForm().findField('CRDOCNO').setValue('');

                    var f = CrDocForm.getForm();
                    f.findField('CRDOCNO').setValue('');
                    f.findField('ACKMMCODE').setValue('');
                    f.findField('MMNAME_C').setValue('');
                    f.findField('MMNAME_E').setValue('');
                    f.findField('APPQTY').setValue('');
                    f.findField('BASE_UNIT').setValue('');
                    f.findField('INQTY').setValue('');
                    f.findField('WH_NAME').setValue('');
                    f.findField('CR_STATUS_NAME').setValue('');
                    f.findField('CR_STATUS').setValue('');
                    f.findField('LOT_NO').setValue('');
                    f.findField('EXP_DATE').setValue('');

                    Ext.getCmp('scanInsert').disable();
                    Ext.getCmp('insert').disable();
                    Ext.getCmp('importData').disable();
                    Ext.getCmp('update').disable();
                    Ext.getCmp('delete').disable();

                    T1Store.removeAll();

                    T1Query.getForm().findField('CRDOCNO').focus();

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);

                    T1Query.getForm().findField('CRDOCNO').setValue(crdocno);
                    MasterLoad(crdocno);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function importData(crdocno) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Import',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("載入成功");

                    MasterLoad(crdocno);

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    //#region 查詢緊急醫療訂單資料
    var callableWin = null;
    popWinForm = function (url) {
        var strUrl = url;
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
                    }
                }]
            });
            var title = '查詢緊急醫療訂單資料';
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }
    //#endregion

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
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
                        height: '100%',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });

    T1Query.getForm().findField('CRDOCNO').focus();
});