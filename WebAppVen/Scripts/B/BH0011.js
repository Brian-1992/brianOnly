Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var reportUrl = '/Report/B/BH0011.aspx';

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'REQDATE', type: 'string' },
            { name: 'CR_UPRICE', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'EMAIL', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'AGEN_TEL', type: 'string' },
            { name: 'AGEN_BOSS', type: 'string' },
            { name: 'REPLYTIME', type: 'string' },
            { name: 'REPLY_STATUS', type: 'string' },
            { name: 'INQTY', type: 'string' },
            { name: 'WEXP_ID', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'IN_STATUS', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'IN_STATUS_TEXT', type: 'string' },
        ]
    });

    var T1Set = '';
    var T1LastRec = null;
    var maxExpDate = '';

    var crDocnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'KEY_CODE']
    });

    function setCombo() {
        Ext.Ajax.request({
            url: '/api/BH0011/GetCrDocnoCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temps = data.etts;

                    for (var i = 0; i < temps.length; i++) {
                        crDocnoStore.add(temps[i]);
                    }
                }
                
            },
            failure: function (response, options) {

            }
        });
    }
    setCombo();
    function getMaxExpDate() {
        Ext.Ajax.request({
            url: '/api/BH0002/GetMaxExpDate',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    maxExpDate = data.msg;
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }
    getMaxExpDate()
    
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
            url: '/api/BH0011/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    crdocno: T1Query.getForm().findField('CRDOCNO').getValue(),
                    start_date: T1Query.getForm().findField('START_DATE').rawValue,
                    end_date: T1Query.getForm().findField('END_DATE').rawValue,
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 120;
    var mWidth = 300;
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
                        padding: '0 0 5 0',
                        items: [
                            {
                                xtype: 'combo',
                                store: crDocnoStore,
                                name: 'CRDOCNO',
                                id: 'CRDOCNO',
                                fieldLabel: '緊急醫療出貨單編號',
                                displayField: 'VALUE',
                                valueField: 'KEY_CODE',
                                queryMode: 'local',
                                anyMatch: true,
                                allowBlank: true,
                                typeAhead: true,
                                triggerAction: 'all',
                                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div></tpl>',
                            },
                            {
                                xtype: 'datefield',
                                fieldLabel: '到貨日期',
                                name: 'START_DATE',
                                //id: 'D0',
                                allowBlank: false, // 欄位為必填
                                fieldCls: 'required',
                                regex: /\d{7,7}/,
                                labelWidth: 90,
                                width:170,
                                enforceMaxLength: true, // 限制可輸入最大長度
                                maxLength: 7, // 可輸入最大長度為7
                                padding: '0 4 0 4',
                                minValue: new Date(),
                                value: new Date(),
                                listeners: {
                                    change: function (self, newValue, oldValue, eOpts) {
                                        if (newValue == null || newValue == '') {
                                            T1Query.getForm().findField('START_DATE').setValue(new Date());
                                        }
                                    }
                                }
                            },
                            {
                                xtype: 'datefield',
                                fieldLabel: '至',
                                name: 'END_DATE',
                                labelSeparator: '',
                                //id: 'D0',
                                allowBlank: true, // 欄位為必填
                                regex: /\d{7,7}/,
                                labelWidth: 10,
                                width: 90,
                                enforceMaxLength: true, // 限制可輸入最大長度
                                maxLength: 7, // 可輸入最大長度為7
                                padding: '0 4 0 4',
                                minValue: new Date(),
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                margin: '0 30 0 0',
                                handler: function () {
                                    if (T1Query.getForm().isValid() == false) {
                                        Ext.Msg.alert('訊息提示', '請檢查輸入條件是否正確');

                                        
                                    } else {
                                        T1Load();
                                        msglabel('');
                                    }
                                }
                            },
                           
                        ]
                    }
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
                itemId: 'update', text: '修改', disabled: true,
                id: 'update',
                handler: function () {
                    setFormT1('U', '修改');
                }
            },
            {
                itemId: 'confirm', text: '確認回傳', disabled: true,
                id: 'confirm',
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('訊息提示', '請先選擇需回傳資料');
                        return;
                    }
                    if (T1LastRec.data.IN_STATUS != 'A') {
                        Ext.Msg.alert('訊息提示', '非可回傳狀態，請重新確認');
                        return;
                    }
                    if (T1LastRec.data.INQTY == '0') {
                        Ext.Msg.alert('訊息提示', '數量為0，請重新確認');
                        return;
                    }
                    if (T1LastRec.data.LOT_NO == '' || T1LastRec.data.LOT_NO == null ||
                        T1LastRec.data.EXP_DATE == '' || T1LastRec.data.EXP_DATE == null) {
                        Ext.Msg.alert('訊息提示', '批號效期未填，請重新確認');
                        return;
                    }
                    Ext.MessageBox.confirm('提示', '是否確定回傳?', function (btn, text) {
                        if (btn === 'yes') {
                            confirm();
                        }
                    });
                }
            },
            {
                itemId: 'print', text: '列印出貨三聯單', disabled: true,
                id: 'print',
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('訊息提示', '請選擇緊急醫療出貨單');
                        return;
                    }
                    showReport(T1LastRec.data.CRDOCNO);
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
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "狀態",
                dataIndex: 'IN_STATUS_TEXT',
                width: 110,
            }, 
            {
                text: "要求到貨日期",
                dataIndex: 'REQDATE',
                width: 110,
            },
            {
                text: "緊急醫療出貨單編號",
                dataIndex: 'CRDOCNO',
                width: 130,
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 180
            },
            {
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
            },
            {
                text: "入庫庫房名稱",
                dataIndex: 'WH_NAME',
                width: 100,
            },
            {
                text: "單價",
                dataIndex: 'CR_UPRICE',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "出單日期",
                dataIndex: 'APPTIME',
                width: 130,
            },
            {
                text: "批號效期註記",
                dataIndex: 'WEXP_ID',
                width: 100,
                align: 'right',
                hidden:true
            },
            {
                text: "進貨數量",
                dataIndex: 'INQTY',
                width: 100,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 130,
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 130,
            },
            {
                text: "病人姓名",
                dataIndex: 'PATIENTNAME',
                width: 130,
            },
            {
                text: "病歷號",
                dataIndex: 'CHARTNO',
                width: 130,
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

                Ext.getCmp('update').disable();
                Ext.getCmp('confirm').disable();
                Ext.getCmp('print').disable();

                if (record.data.IN_STATUS == 'A') {
                    Ext.getCmp('update').enable();
                    Ext.getCmp('confirm').enable();
                } else if (record.data.IN_STATUS == 'B'){
                    Ext.getCmp('print').enable();

                } else if (record.data.IN_STATUS == 'C') {
                    Ext.getCmp('print').enable();
                }

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
                fieldLabel: '緊急醫療出貨單編號',
                name: 'CRDOCNO',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '院內碼',
                name: 'MMCODE',
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
                fieldLabel: '申請數量',
                name: 'APPQTY',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                xtype: 'displayfield'
            },
            {
                fieldLabel: 'SEQ',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                fieldLabel: '進貨數量',
                name: 'INQTY',
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true,
                
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
                //allowBlank: false // 欄位為必填
               
                
            },
            {
                xtype: 'textfield',
                name: 'WEXP_ID', // Y=效期管制  
                id: 'WEXP_ID',
                hidden: true
            },
            {
                fieldLabel: '病人姓名',
                name: 'PATIENTNAME',
                xtype: 'displayfield'
            },
            {
                fieldLabel: '病歷號',
                name: 'CHARTNO',
                xtype: 'displayfield'
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    var f = T1Form.getForm();
                    var msg = "";
                    var WEXP_ID = f.findField("WEXP_ID").getValue();
                    var LOT_NO = f.findField("LOT_NO").getValue();
                    var EXP_DATE = f.findField("EXP_DATE").getValue();
                    var INQTY = f.findField("INQTY").getValue();

                    if (INQTY <= 0) {
                        msg += "[進貨數量]不可為0";
                    }
                    if (INQTY > f.findField('APPQTY').getValue()) {
                        msg += "[進貨數量]不可超過申請數量";
                    }
                    if (LOT_NO == "" || EXP_DATE == null) {
                        msg += "[效期],[批號]不可以空白";
                    }
                    
                    
                    if (msg != "") {
                        Ext.Msg.alert('訊息提示', msg);
                        return;
                    }
                    debugger
                    if (new Date(EXP_DATE) > new Date(maxExpDate)) {
                        msg = '效期超過最大值，已更新為最大效期，請確認';
                        f.findField("EXP_DATE").setValue(new Date(maxExpDate));
                        Ext.Msg.alert('訊息提示', msg);
                        return;
                    }

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
            debugger
            Ext.Ajax.request({
                url: '/api/BH0011/Update',
                method: reqVal_p,
                params: {
                    crdocno: f.findField('CRDOCNO').getValue(),
                    inqty: f.findField('INQTY').getValue(),
                    lot_no: f.findField('LOT_NO').getValue(),
                    exp_date: f.findField('EXP_DATE').rawValue,
                },
                success: function (response) {
                    debugger
                    console.log(response);
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        myMask.hide();
                        viewport.down('#form').collapse();
                        
                        msglabel("修改成功");
                        debugger
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
        //u.focus();
    }

    function setFormT1a() {

        if (T1LastRec != null) {
            viewport.down('#form').expand();
            viewport.down('#form').setTitle('瀏覽');

            T1Form.loadRecord(T1LastRec);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    function showReport(crdocno) {
        if (!win) {
            var np = {
                crdocno: crdocno
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?crdocno=' + np.crdocno + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    function confirm() {
        var crdocno = T1LastRec.data.CRDOCNO;

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        
        Ext.Ajax.request({
            url: '/api/BH0011/Confirm',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();
                    
                    msglabel("回傳成功");
                    T1Query.getForm().findField('CRDOCNO').setValue(T1LastRec.data.CRDOCNO);
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
});