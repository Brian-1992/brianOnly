Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1LastRec, T2LastRec;
    var T1Name = "繳回管理";
    var userId = session["UserId"];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var userTaskId;

    //T1Model        //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },        // 繳回單號
            { name: 'MAT_CLASS', type: 'string' },           // 物料分類
            { name: 'APPTIME', type: 'string' },         // 繳回申請時間
            { name: 'TOWH', type: 'string' }, // 入庫庫房
            { name: 'FRWH', type: 'string' },       // 出庫庫房
            { name: 'FLOWID', type: 'string' },      // 單據狀態
            { name: 'APPLY_NOTE', type: 'string' },      // 備註
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0149/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;

        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
        T1Store.getProxy().setExtraParam("TASKID", userTaskId);
        T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Tool.moveFirst();

        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    //T2Model        //定義有多少欄位參數
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },        // 繳回單號
            { name: 'SEQ', type: 'string' },        // 項次
            { name: 'MMCODE', type: 'string' },           // 院內碼
            { name: 'APPQTY', type: 'string' },       // 繳回數量
            { name: 'MMNAME_C', type: 'string' },         // 中文品名
            { name: 'MMNAME_E', type: 'string' }, // 英文品名
            { name: 'BASE_UNIT', type: 'string' },        // 計量單位
            { name: 'INV_QTY', type: 'string' },      // 出庫庫房存量
            { name: 'FLOWID', type: 'string' },      // 單據狀態
            { name: 'APLYITEM_NOTE', type: 'string' },      // 備註
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 50,
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0149/AllMeDocd',
            reader: {
                type: 'json',
                rootProperty: 'etts'
                //totalProperty: 'rc'
            }
        }
    });
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
            //T2Store.moveFirst();
        }
    }

    var FlowIdStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function FlowIdLoad() {
        Ext.Ajax.request({
            url: '/api/AA0149/GetFlowidCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var records = data.etts;
                    if (records.length > 0) {
                        FlowIdStore.removeAll();
                        var arrP2 = [];
                        for (var i = 0; i < records.length; i++) {
                            FlowIdStore.add({
                                VALUE: records[i].VALUE,
                                TEXT: records[i].TEXT,
                                COMBITEM: records[i].COMBITEM
                            });
                            if (records[i].VALUE == '2')
                                arrP2.push("2");
                            if (records[i].VALUE == '0402')
                                arrP2.push("0402");
                        }
                        T1Query.getForm().findField('P2').setValue(arrP2);
                    }
                }
            }
        });
    }

    var MatClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var MatClassStoreQ = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function MatClassLoad() {
        Ext.Ajax.request({
            url: '/api/AA0149/GetMatClassCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var records = data.etts;
                    if (records.length > 0) {
                        MatClassStore.removeAll();
                        for (var i = 0; i < records.length; i++) {
                            MatClassStore.add({
                                VALUE: records[i].VALUE,
                                TEXT: records[i].TEXT,
                                COMBITEM: records[i].COMBITEM
                            });
                        }
                    }
                }
            }
        });

        Ext.Ajax.request({
            url: '/api/AA0149/GetMatClassQCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var records = data.etts;
                    if (records.length > 0) {
                        MatClassStoreQ.removeAll();
                        for (var i = 0; i < records.length; i++) {
                            MatClassStoreQ.add({
                                VALUE: records[i].VALUE,
                                TEXT: records[i].TEXT,
                                COMBITEM: records[i].COMBITEM
                            });
                        }
                    }
                }
            }
        });
    }

    var st_inidcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0149/GetInidComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0149/GetFrwhComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P3',
                id: 'P3',
                store: MatClassStoreQ,
                labelWidth: 90,
                editable: false,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            },
            {
                xtype: 'datefield',
                fieldLabel: '繳回日期',
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
                fieldLabel: '單據狀態',
                name: 'P2',
                id: 'P2',
                store: FlowIdStore,
                labelWidth: 90,
                editable: false,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            },
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    //setFormVisible(true, false);

                    T1Load();
                    Ext.getCmp('eastform').collapse();
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    //f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    setT1Matclassdefault();
                }
            }
        ]
    });

    function setSubmit(args) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AA0149/UpdateStatus',
            method: reqVal_p,
            params: {
                DOCNO: args.DOCNO
            },
            //async: true,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('訊息區:繳回成功');
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnReject').setDisabled(true);
                    Ext.getCmp('btnExp').setDisabled(true);

                    // 詢問是否轉開退貨單
                    Ext.MessageBox.confirm('訊息', '是否轉開退貨單?', function (btn) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0149/TransferDoc',
                                method: reqVal_p,
                                params: {
                                    DOCNO: args.DOCNO
                                },
                                success: function (response, opts) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        parent.link2('/Form/Index/AA0151', '退貨作業(AB0125)');
                                    }
                                },
                                failure: function (response, opts) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    })
                } else {
                    Ext.MessageBox.alert('錯誤', ('發生例外錯誤:' + data.msg));
                }
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
                text: '繳回確認',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '請確認是否繳回？' + '單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                setSubmit({
                                    DOCNO: docno
                                });
                            }
                        });
                      
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取繳回單號');
                        return;
                    }
                }
            },
            {
                id: 'btnReject',
                name: 'btnReject',
                text: '退回',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否退回單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0149/MasterReject',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:退回成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnReject').setDisabled(true);
                                        } else {
                                            Ext.MessageBox.alert('錯誤', ('發生例外錯誤:' + data.msg));
                                        }
                                    },
                                    failure: function (response) {
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
            },
            {
                text: '效期',
                id: 'btnExp',
                disabled: true,
                hidden: true,  //1130318改由使用者填寫批號效期
                handler: function () {
                    showExpWindow(T1LastRec.data.DOCNO, 'I', viewport);
                }
            }
        ]
    });
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnReject').setDisabled(true);

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
                text: "繳回單號",
                dataIndex: 'DOCNO',
                width: 150
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 120
            },
            {
                text: "繳回申請時間",
                dataIndex: 'APPTIME',
                width: 150
            },
            {
                text: "申請庫房",
                dataIndex: 'FRWH',
                width: 200
            },
            {
                text: "繳回庫房",
                dataIndex: 'TOWH',
                width: 200
            },
            {
                text: "單據狀態",
                dataIndex: 'FLOWID',
                width: 150
            },
            {
                text: "備註",
                dataIndex: 'APPLY_NOTE',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                if (T1Form.hidden === true) {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                }

                // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                if (T1LastRec != null) {
                    if (T1Rec.length == 0) {
                        T1LastRec = null;
                        T2Store.removeAll();
                        return;
                    }

                    // 所有按鈕預設disabled
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnExp').setDisabled(true);
                    Ext.getCmp('btnReject').setDisabled(true);

                    // 繳回確認：單筆或所有勾選資料狀態為[2-衛材繳回中, 0402-藥品點收中]才可enable
                    // 退回：單筆或所有勾選資料狀態為[2-衛材繳回中, 0402-藥品點收中]才可enable
                    var btnSubmitCheck = 0;
                    var btnRejectCheck = 0;
                    for (var i = 0; i < T1Rec.length; i++) {
                        var tmpRecArray = T1Rec[i].data["FLOWID"].split(' ');
                        if (tmpRecArray[0] !== '2' && tmpRecArray[0] !== '0402') {    // 2-衛材繳回中, 0402-藥品點收中
                            btnSubmitCheck++;
                            btnRejectCheck++;
                        }
                    }
                    if (btnSubmitCheck == 0) {
                        Ext.getCmp('btnSubmit').setDisabled(false);
                    }
                    if (btnRejectCheck == 0) {
                        Ext.getCmp('btnReject').setDisabled(false);
                    }

                    // 效期：明細內有批號效期管制品項才enable
                    if (T1Rec.length == 1) {
                        Ext.Ajax.request({
                            url: '/api/AA0149/CheckExp',
                            method: reqVal_p,
                            params: {
                                DOCNO: T1LastRec.data["DOCNO"],
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    Ext.getCmp('btnExp').setDisabled(false);
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }

                    T2Load();
                    setFormT1a();
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records;
                T1LastRec = records[records.length - 1];
                setFormT1a();
                T2Load();
            },
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
                xtype: 'displayfield',
                fieldLabel: '繳回單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
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
                //id: 'MAT_CLASS',
                store: MatClassStore,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                readOnly: true,
                allowBlank: false, // 欄位為必填
                //anyMatch: true,
                //typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                //triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單據狀態',
                name: 'FLOWID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                hidden: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請庫房',
                name: 'FRWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回庫房',
                name: 'TOWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APPLY_NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
        ]
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            Tabs.setActiveTab('Form');
            var currentTab = Tabs.getActiveTab();
            currentTab.setTitle('瀏覽');

            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH"]);
            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            var tmpArray = T1LastRec.data["MAT_CLASS"].split(" ");
            f.findField('MAT_CLASS').setValue(tmpArray[0]);

            f.findField('FRWH').setReadOnly(true);
            f.findField('TOWH').setReadOnly(true);
            f.findField('MAT_CLASS').setReadOnly(true);
            f.findField('FLOWID').setVisible(true);
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        hidden: true,
        cls: 'T2b',
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
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回單號',
                name: 'DOCNO2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 8,
                readOnly: true,
                submitValue: true,
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
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '出庫庫房存量',
                name: 'INV_QTY',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APLYITEM_NOTE',
                labelWidth: 90,
                labelAlign: 'right',
                readOnly: true,
                allowBlank: true,
                maxLength: 50,
            }
        ]
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            //viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField("MMNAME_C").setValue(T2LastRec.data["MMNAME_C"]);
            f.findField("MMNAME_E").setValue(T2LastRec.data["MMNAME_E"]);
            f.findField("BASE_UNIT").setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField("APPQTY").setValue(T2LastRec.data["APPQTY"]);
            f.findField("INV_QTY").setValue(T2LastRec.data["INV_QTY"]);
            f.findField("APLYITEM_NOTE").setValue(T2LastRec.data["APLYITEM_NOTE"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
        }

    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "繳回單號",
                dataIndex: 'DOCNO',
                width: 150
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "繳回數量",
                //text: "<span style='color:red'>繳回數量</span>",
                dataIndex: 'APPQTY',
                width: 100,
                align: 'right',
                //style: 'text-align:left',
                //format: '0.000',
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 350
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 350
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: '出庫庫房存量',
                dataIndex: 'INV_QTY',
                width: 120,
                align: 'right',
                //style: 'text-align:left',
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 150
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
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records;
                T2LastRec = records[records.length - 1];
                setFormT2a();
            },
        }
    });

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
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
                title: '查詢',
                //border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [Tabs]
                //items: [T1Query]
            }
        ]
    });

    //Ext.getCmp('eastform').collapse();
    Ext.getCmp('eastform').expand();
    //T1Load();
    MatClassLoad();
    FlowIdLoad();
});