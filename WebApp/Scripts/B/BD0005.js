
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "訂單傳真";
    var reportUrl = '/Report/B/BD0005.aspx'; // '/Report/A/AA0013.aspx';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var PO_NO = "";
    var MATComboGet = '../../../api/BA0002/GetMATCombo';  
    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1QueryForm.getForm().findField('P4').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboData();  
    // 共用函式
    function getAddDate(addDays) {
        var dt = new Date();
        dt.setDate(dt.getDate() + addDays);
        return dt;
    }

    function getFirstDayOfMonth() {
        var dt = new Date();
        dt.setDate(dt.getDate() + addDays);
        return dt;
    }
    
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear() - 1911;
        var m = new Date().getMonth() + 1;
        var d = 0;
        if (isEndDate) {
            d = new Date(yyyy, m, 0).getDate();
        } else {
            d = 1;
        }
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();
        return yyyy.toString() + mm + dd;
    }   
    // 查詢欄位

    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        title: '',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60
        },        
        items: [
            {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 7 7 0',
                items: [
                    {
                    xtype: 'datefield',
                    id: 'TWN_DATE_START',
                    name: 'TWN_DATE_START',
                    fieldLabel: '申請日期',
                    labelWidth: 60,
                    width: 150,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    value: getDefaultValue(false),
                    regexText: '請選擇日期'
                }, {
                xtype: 'datefield',
                id: 'TWN_DATE_END',
                name: 'TWN_DATE_END',
                fieldLabel: '至',
                width: 88,
                labelWidth: 8,
                padding: '0 4 0 4',
                allowBlank: false,
                fieldCls: 'required',
                value: getDefaultValue(true),
                regexText: '請選擇日期'
                    }, {
                        xtype: 'radiogroup',
                        anchor: '40%',
                        labelWidth: 60,
                        fieldLabel: '合約種類',
                        width: 240,
                        items: [
                            { boxLabel: '合約', width: 50, name: 'M_CONTID', inputValue: '0', checked: true },
                            { boxLabel: '非合約', width: 60, name: 'M_CONTID', inputValue: '2' }
                        ]
                    }, {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'P4',
                        enforceMaxLength: true,
                        labelWidth: 60,
                        width: 170,
                        padding: '0 4 0 4',
                        store: MATQueryStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    }, {
                        xtype: 'button',
                        itemId: 'T1query', text: '查詢',
                        iconCls: 'TRASearch',
                        handler: function () {
                            PO_NO = "";
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'T1Print', disabled: true, text: '列印傳真',
                        handler: function () {
                            showReport();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }            
        ]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: '訂單編號', type: 'string' }, // 01.PO_NO
            { name: '廠商', type: 'string' }, // 00.AGEN_NO
            { name: '訂單狀態', type: 'string' }, // 00.PO_STATUS
            { name: 'MAIL備註', type: 'string' }, // 00.MEMO
            //{ name: '', type: 'string' }, // 00.ISCONFIRM 
            { name: 'MAIL特殊備註', type: 'string' }, // 00.SMEMO
            { name: 'UPDATE_USER', type: 'string' }, 
            { name: 'UPDATE_IP', type: 'string' } 
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO, AGEN_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0005/AllM',
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
                    TWN_DATE_START: T1QueryForm.getForm().findField('TWN_DATE_START').rawValue,
                    TWN_DATE_END: T1QueryForm.getForm().findField('TWN_DATE_END').rawValue,
                    M_CONTID: T1QueryForm.getForm().getValues()['M_CONTID'],
                    p4: T1QueryForm.getForm().findField('P4').getValue(),
                    ENDL:''
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BD0005/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
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
        f.findField('x').setValue(x);
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
        dockedItems: [
            {
                itemId: 'Query',
                title: '查詢',
                items: [T1QueryForm]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單編號", dataIndex: 'PO_NO', width: 150, sortable: true
        }, {
            text: "廠商", dataIndex: 'AGEN_NO', width: 150, sortable: true
        }, {
            text: "訂單狀態", dataIndex: 'PO_STATUS', width: 150, sortable: true
        }, {
            text: "Mail備註", dataIndex: 'MEMO', width: 150, sortable: true
        }, {
            text: "Mail特殊備註", dataIndex: 'SMEMO', width: 150, sortable: true
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
        T1QueryForm.down('#T1Print').setDisabled(T1Rec === 0);

        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            f.findField('MEMO').setReadOnly(false);
            f.findField('SMEMO').setReadOnly(false);
            PO_NO = f.findField('PO_NO').getValue();
            f.findField('PO_NO_LABEL').setValue(f.findField('PO_NO').getValue());
            T1Grid.down('#edit').setDisabled(false);
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }
        T2Cleanup();
        //T2Query.getForm().reset();
        T2Load();
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: { type: 'table', columns: 1 },
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
            name: 'PO_NO',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield', fieldLabel: '訂單編號', name: 'PO_NO_LABEL'
        }, {
            xtype: 'displayfield', fieldLabel: '廠商', name: 'AGEN_NO'
        }, {
            xtype: 'displayfield', fieldLabel: '訂單狀態', name: 'PO_STATUS'
        }, {
            xtype: 'textareafield',
            fieldLabel: 'Mail備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            height: 200,
            readOnly: true,
            width: "100%"
        }, {
            xtype: 'textareafield',
            fieldLabel: 'Mail特殊備註',
            name: 'SMEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            height: 200,
            readOnly: true,
            width: "100%"
        }, {
                name: 'UPDATE_USER',
                xtype: 'hidden'
        }, {
                name: 'UPDATE_IP',
                xtype: 'hidden'
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
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
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
        // T1QueryForm.getForm().findField('P2').setValue("1");
        TATabs.setActiveTab('Query');
    }

    function showReport() {
        if (!win) {
            var M_CONTID = T1QueryForm.getForm().findField('M_CONTID').getValue();
            if (M_CONTID == "true")
                M_CONTID = "0"
            else
                M_CONTID = "1";
            var np = {
                PO_NO: T1Form.getForm().findField('PO_NO').getValue(),
                M_CONTID: M_CONTID,
                Action: 3
            };
            UPDATE_USER = T1Form.getForm().findField('UPDATE_USER').getValue();
            UPDATE_IP = T1Form.getForm().findField('UPDATE_IP').getValue();
            USERDATA = '&UPDATE_USER=' + UPDATE_USER + '&UPDATE_IP=' + UPDATE_IP; 
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?&PO_NO=' + np.PO_NO + '&M_CONTID=' + np.M_CONTID + USERDATA + '&Action=' + np.Action + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        T1Load();
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
            { name: 'MMCODE', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0005/AllD',
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

                    PO_NO: PO_NO, // T1Form.getForm().findField('PO_NO').getValue(),
                    ENDL: ''
                    //p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
        //try {
        //    T2Store.load({
        //        params: {
        //            start: 0
        //        }
        //    });
        //}
        //catch (e) {
        //    alert("T2Load Error:" + e);
        //}
        viewport.down('#form').collapse();
    }
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
            xtype: 'displayfield', fieldLabel: '院內碼', name: 'MMCODE'
        }, {
            xtype: 'displayfield', fieldLabel: '中文品名', name: 'MMNAME_C'
        }, {
            xtype: 'displayfield', fieldLabel: '英文品名', name: 'MMNAME_E'
        }, {
            xtype: 'displayfield', fieldLabel: '廠牌', name: 'M_AGENLAB'
        }, {
            xtype: 'displayfield', fieldLabel: '單位', name: 'M_PURUN'
        }, {
            xtype: 'displayfield', fieldLabel: '單價', name: 'PO_PRICE'
        }, {
            xtype: 'displayfield', fieldLabel: '申購量', name: 'PO_QTY'
        }, {
            xtype: 'displayfield', fieldLabel: '單筆價', name: 'PO_AMT'
        }, {
            xtype: 'displayfield', fieldLabel: '備註', name: 'MEMO'
        }]
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
        items: [
            {
                itemId: 'Form',
                title: '瀏覽',
                items: [T1Form, T2Form]
            }
        ]
    });
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
        //f.findField('MMCODE').setReadOnly(true);
        //f.findField('APPQTY').setReadOnly(true);
        //f.findField('APLYITEM_NOTE').setReadOnly(true);
        //T2Form.down('#T2Cancel').hide();
        //T2Form.down('#T2Submit').hide();
        //T2Form.down('#btnMmcode').setVisible(false);
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼", dataIndex: 'MMCODE', width: 100, sortable: true
        }, {
            text: "中文品名", dataIndex: 'MMNAME_C', width: 100, sortable: true
        }, {
            text: "英文品名", dataIndex: 'MMNAME_E', width: 100, sortable: true
        }, {
            text: "廠牌", dataIndex: 'M_AGENLAB', width: 100, sortable: true
        }, {
            text: "單位", dataIndex: 'M_PURUN', width: 100, sortable: true
        }, {
            text: "單價", dataIndex: 'PO_PRICE', width: 100, sortable: true
        }, {
            text: "申購量", dataIndex: 'PO_QTY', width: 100, sortable: true
        }, {
            text: "單筆價", dataIndex: 'PO_AMT', width: 100, sortable: true
        }, {
            text: "備註", dataIndex: 'MEMO', width: 100, sortable: true
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
                T2Store.removeAll();    
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        //T2Grid.down('#edit').setDisabled(T2Rec === 0);
        //T2Grid.down('#delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            //f.findField('DATA_SEQ_O').setValue(T2LastRec.get('DATA_SEQ'));
            //var u = f.findField('ID');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            //if (T1F3 === '1') {
            //    T2Grid.down('#edit').setDisabled(false);
            //    T2Grid.down('#delete').setDisabled(false);
            //}
            //else {
            //    T2Grid.down('#edit').setDisabled(true);
            //    T2Grid.down('#delete').setDisabled(true);
            //}
        }
        else {
            T2Form.getForm().reset();
        }
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

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;

    //T1Load(); // 進入畫面時自動載入一次資料
    viewport.down('#form').setTitle('瀏覽');
    viewport.down('#form').collapse();
});
