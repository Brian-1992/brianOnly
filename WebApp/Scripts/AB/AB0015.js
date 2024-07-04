Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0015/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "申請人員點收";
    var reportUrl = '/Report/A/AA0013.aspx';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var T1_doctype = "";

    var isCR = '';

    var arrP3 = ["3", "4", "5","51"]; 
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0015/GetDocnoCombo',
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
            url: '/api/AB0015/GetFlowidCombo',
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
            url: '/api/AB0015/GetApplyKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_appdept = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0015/GetAppDeptCombo',
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
            url: '/api/AB0015/GetMatclassCombo',
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
            url: '/api/AB0015/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_storeid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0015/GetStoreIdCombo',
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
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'P0',
                id: 'P0',
                vtype: 'dateRange',
                dateRange: { end: 'P1' },
                padding: '0 4 0 4'
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'P1',
                id: 'P1',
                vtype: 'dateRange',
                dateRange: { begin: 'P0' },
                padding: '0 4 0 4'
            //}, {
            //    xtype: 'combo',
            //    fieldLabel: '申請單位',
            //    name: 'P2',
            //    id: 'P2',
            //    store: st_appdept,
            //    queryMode: 'local',
            //    displayField: 'COMBITEM',
            //    valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單狀態',
                name: 'P3',
                id: 'P3',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單分類',
                name: 'P4',
                id: 'P4',
                store: st_apply_kind,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P5',
                id: 'P5',
                store: st_matclass,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '庫備識別',
                name: 'P6',
                id: 'P6',
                store: st_storeid,
                queryMode: 'local',
                displayField: 'TEXT',
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
                T1QueryForm.getForm().findField('P3').setValue(arrP3);
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
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_DATE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MAT_CLASS_N', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'WEXP_YN', type: 'string' },
            { name: 'DOCTYPE_N', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 5, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0015/AllM',
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
                    p0: T1QueryForm.getForm().findField('P0').rawValue,
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    //p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p4: T1QueryForm.getForm().findField('P4').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue(),
                    p6: T1QueryForm.getForm().findField('P6').getValue()
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
                itemId: 'apply', text: '點收', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else
                    {
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
                        Ext.MessageBox.confirm('點收', '是否確定點收？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0015/Apply',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '送出成功');
                                            msglabel('訊息區:點收成功');
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
            }, {
                itemId: 'exp', text: '效期', disabled: true, handler: function () {
                    showExpWindow(T1F1, 'I', viewport);
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
        //viewport.down('#form').expand();
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
            f.findField('APPTIME_T').setValue('');
            f.findField('FRWH_N').setValue('');
            f.findField('FLOWID').setValue('1');
            f.findField('DOCTYPE').setValue('MR2');
            f.findField('APPLY_KIND_N').setValue('');
            f.findField('MAT_CLASS').setValue(st_matclass.getAt(0).get('VALUE'));
            f.findField('TOWH').setValue(st_towhcombo.getAt(0).get('VALUE'));

        }
        else {
            u = f.findField('DOCNO');
        }
        f.findField('x').setValue(x);
        f.findField('DOCNO').setReadOnly(false);
        f.findField('FLOWID').setReadOnly(false);
        f.findField('APPLY_NOTE').setReadOnly(false);
        f.findField('MAT_CLASS').setReadOnly(false);
        f.findField('TOWH').setReadOnly(false);
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
            text: "類別",
            dataIndex: 'APPLY_KIND_N',
            width: 80
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
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
            text: "庫備識別",
            dataIndex: 'DOCTYPE_N',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'ISCRNOTE',
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
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                isCR = 'N';
                if (T1LastRec != null) {
                    isCR = T1LastRec.data.ISCR;
                }
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        //T1Grid.down('#apply').setDisabled(T1Rec === 0);
        //T1Grid.down('#exp').setDisabled(T1Rec === 0);
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        //viewport.down('#form').expand();
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
            T1_doctype = T1LastRec.data.DOCTYPE;
            var T1_isdis = T1LastRec.data.isDis;
            if (T1F3 === '3' || T1F3 === '4') {
                T1Grid.down('#apply').setDisabled(false);
            }
            else {
                T1Grid.down('#apply').setDisabled(true);
            }
            if (f.findField('WEXP_YN').getValue() == 'Y') {
                T1Grid.down('#exp').setDisabled(false);
            }
            else {
                T1Grid.down('#exp').setDisabled(true);
            }

            // T2Grid非庫備欄位
            var indexAPVQTY_0 = findColumnIndexByName(T2Grid.columns, 'APVQTY_0');
            var indexACKQTYT = findColumnIndexByName(T2Grid.columns, 'ACKQTYT');
            var indexEXPT_DISTQTY_0 = findColumnIndexByName(T2Grid.columns, 'EXPT_DISTQTY_0');
            var indexPICK_QTY_0 = findColumnIndexByName(T2Grid.columns, 'PICK_QTY_0');
            // T2Grid庫備欄位
            var indexEXPT_DISTQTY_1 = findColumnIndexByName(T2Grid.columns, 'EXPT_DISTQTY_1');
            var indexAPVQTY_1 = findColumnIndexByName(T2Grid.columns, 'APVQTY_1');
            var indexPICK_QTY_1 = findColumnIndexByName(T2Grid.columns, 'PICK_QTY_1');

            var indexACKQTY = findColumnIndexByName(T2Grid.columns, 'ACKQTY');

            if ((T1_doctype == 'MR3' || T1_doctype == 'MR4') && T1_isdis == 'N') {
                T2Grid.getColumns()[indexAPVQTY_0].show();
                T2Grid.getColumns()[indexACKQTYT].show();
                T2Grid.getColumns()[indexEXPT_DISTQTY_0].show();
                T2Grid.getColumns()[indexPICK_QTY_0].show();
                T2Grid.getColumns()[indexEXPT_DISTQTY_1].hide();
                T2Grid.getColumns()[indexAPVQTY_1].hide();
                T2Grid.getColumns()[indexPICK_QTY_1].hide();

                T2Grid.getColumns()[indexACKQTY].setText('<b><font color=red>本次點收</font></b>');

                Ext.getCmp('docd0Info').show();
            } else {
                T2Grid.getColumns()[indexAPVQTY_0].hide();
                T2Grid.getColumns()[indexACKQTYT].hide();
                T2Grid.getColumns()[indexEXPT_DISTQTY_0].hide();
                T2Grid.getColumns()[indexPICK_QTY_0].hide();
                T2Grid.getColumns()[indexEXPT_DISTQTY_1].show();
                T2Grid.getColumns()[indexAPVQTY_1].show();
                T2Grid.getColumns()[indexPICK_QTY_1].show();

                T2Grid.getColumns()[indexACKQTY].setText('<b><font color=red>點收量</font></b>');

                Ext.getCmp('docd0Info').hide();
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
            name: 'TOWH',
            xtype: 'hidden'
        }, {
            name: 'APPLY_KIND',
            xtype: 'hidden'
        }, {
            name: 'MAT_CLASS',
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
            name: 'WEXP_YN',
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
            fieldLabel: '申請時間',
            name: 'APPTIME_T'
        }, {
            xtype: 'displayfield',
            fieldLabel: '出庫庫房',
            name: 'FRWH_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '入庫庫房',
            name: 'TOWH_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE'
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
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料點收成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
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
        T1QueryForm.getForm().findField('P3').setValue(arrP3);
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0015/GetMMCodeDocd', //指定查詢的Controller路徑
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
                name: 'P2',
                xtype: 'hidden'
            }, {
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
                fieldLabel: '僅顯示差異',
                name: 'P02',
                xtype: 'checkboxfield',
                labelWidth: 80,
                labelAlign: 'right',
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T2Query.getForm().findField('P2').setValue("N");
                            T2Load();
                        } else {
                            T2Query.getForm().findField('P2').setValue("Y");
                            T2Load();
                        }
                    }
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '※非庫備申請單院內碼底色為紅色者為本次可點收之品項',
                id: 'docd0Info',
                labelWidth: 350,
                labelSeparator: '',
                labelStyle: 'font-weight: bold; color: blue;',
                hidden: true,
                padding: '0 0 0 5'
            },
        ]
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
            { name: 'BW_MQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'ONWAY_QTY', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMCODE_N', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'AVG_APLQTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'TOT_APVQTY', type: 'string' },
            { name: 'TOT_BWQTY', type: 'string' },
            { name: 'TOT_DISTUN', type: 'string' },
            { name: 'ONWAYQTY', type: 'string' },
            { name: 'AFTERQTY', type: 'string' },
            { name: 'BW_SQTY', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            
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
            url: '/api/AB0015/AllD',
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
                    p1: T2Query.getForm().findField('P1').getValue(),
                    p2: T2Query.getForm().findField('P2').getValue() 
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
    var findColumnIndexByName = function (columns, columnName) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].name == columnName) { break; }
        }
        return index == columns.length ? -1 : index;
    }

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
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
        }, {
            name: 'PICK_QTY',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE'
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
            fieldLabel: '計量單位',
            name: 'BASE_UNIT'
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨單價',
            name: 'M_CONTPRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存單價',
            name: 'AVG_PRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '安全存量',
            name: 'SAFE_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存數量',
            name: 'INV_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '平均申請數量',
            name: 'AVG_APLQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請數量',
            name: 'APPQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '預計核撥量',
            name: 'EXPT_DISTQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '調撥量',
            name: 'BW_MQTY'
        }, {
            fieldLabel: '點收數量',
            name: 'ACKQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '實際核撥量',
            name: 'APVQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '核撥時間',
            name: 'APVTIME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '月累計量',
            name: 'TOT_APVQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '累計調撥量',
            name: 'TOT_BWQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '撥發包裝',
            name: 'TOT_DISTUN'
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                //if (this.up('form').getForm().findField('ACKQTY').getValue() == '0')
                //    Ext.Msg.alert('提醒', '點收數量不可為0');
                //else {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    }
                    );
                //}
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
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
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
        f.findField('ACKQTY').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        //viewport.down('#form').expand();
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
        }
        else {
            u = f2.findField('ACKQTY');
            var pick_qty = 0;
            var bw_sqty = 0;
            var ack_qty = 0;
            if (T2LastRec.get('ACKQTY') == "" || T2LastRec.get('ACKQTY') == "0") {
                pick_qty = T2LastRec.get('PICK_QTY');
                bw_sqty = T2LastRec.get('BW_SQTY');
                ack_qty = pick_qty - bw_sqty;
                f2.findField('ACKQTY').setValue(ack_qty);
            }
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('ACKQTY').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "",
            dataIndex: 'SGN',
            align: 'center',
            style: 'text-align:left',
            width: 30,
            renderer: function (val, meta, record) {
                if (val == '1') {
                    return '<span style="font-size:25px;color:red">●</span>';
                }
            }
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true,
            renderer: function (value, metaData, record, rowIndex) {
                if ((Number(record.data.ONWAYQTY) > 0 || Number(record.data.PICK_QTY) > 0) &&
                    (record.data.M_STOREID) && T1F3 == '4') {
                    metaData.tdStyle = 'background-color:#ffaaaa'; //'<span>' + value + '</span>';
                } 

                return value;
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 100,
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
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "安全存量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "庫存量",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "申請量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 70, align: 'right'
        },
        {
            text: "累計核撥量",      // 非庫備
            dataIndex: 'APVQTY',
            name: 'APVQTY_0',
            style: 'text-align:left',
            width: 100, align: 'right',
            hidden:true
        },
        {
            text: "累計點收",       // 非庫備
            dataIndex: 'ACKQTYT',
            name: 'ACKQTYT',
            style: 'text-align:left',
            width: 100, align: 'right',
            hidden: true
        },
        {
            text: "預計核撥量",      
            dataIndex: 'EXPT_DISTQTY',
            name: 'EXPT_DISTQTY_1',
            style: 'text-align:left',
            width: 100, align: 'right'
        },
        {
            text: "本次核撥量",      // 非庫備
            dataIndex: 'EXPT_DISTQTY',
            name: 'EXPT_DISTQTY_0',
            style: 'text-align:left',
            width: 100, align: 'right',
            hidden: true
        },
        {
            text: "<b><font color=red>點收量</font></b>",
            dataIndex: 'ACKQTY',
            style: 'text-align:left',
            width: 70,
            name:'ACKQTY',
            editor: {
            xtype: 'textfield',
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            }, align: 'right',
            renderer: function (value, metaData, record, rowIndex) {
                if ((Number(record.data.ONWAYQTY) > 0 || Number(record.data.PICK_QTY) > 0) &&
                    (record.data.M_STOREID) && T1F3 == '4') {
                    metaData.tdStyle = 'background-color:#ffaaaa'; //'<span>' + value + '</span>';
                } 

                return value;
            }
        }, {
            text: "在途量(未點收)",
            dataIndex: 'ONWAYQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        },
        {
            text: "揀料量",
            dataIndex: 'PICK_QTY',
            name: 'PICK_QTY_1',
            style: 'text-align:left',
            width: 100, align: 'right'
        },
        {
            text: "本次分配",       // 非庫備
            dataIndex: 'PICK_QTY',
            name: 'PICK_QTY_0',
            style: 'text-align:left',
            width: 100, align: 'right'
        },
        {
            text: "實際核撥量",
            dataIndex: 'APVQTY',
            name:'APVQTY_1',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "借貨量",
            dataIndex: 'BW_SQTY',
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "<b><font color=red>備註</font></b>",
            dataIndex: 'APLYITEM_NOTE',
            width: 170,
            maxLength: 50,
            editor: {
                xtype: 'textfield'
            }
        }, {
            text: "點收人員",
            dataIndex: 'ACKID',
            width: 70
        }, {
            text: "點收時間",
            dataIndex: 'ACKTIME',
            width: 100
        }, {
            header: "",
            flex: 1
        }
        ],
        plugins: {
            ptype: 'rowediting',
            clicksToEdit: 1,
            autoCancel: false,
            saveBtnText: '更新',
            cancelBtnText: '取消',
            errorsText: '錯誤訊息',
            dirtyText: '請按更新以修改資料或取消變更',
            listeners: {
                beforeedit: function (editor, context, eOpts) {
                    
                    if (isCR == 'Y') {
                        return false;
                    }
                    if (T1F3 != '3' && T1F3 != '4') {
                        return false;
                    }
                    if ((context.record.data.ONWAYQTY == '0' || context.record.data.ONWAYQTY == '') &&
                        context.record.data.PICK_QTY == '0') {
                        return false;
                    }

                    return true;
                },
                edit: function (editor, context, eOpts) {
                    if (Number(context.record.data.ACKQTY) > Number(context.record.data.PICK_QTY)) {
                        Ext.MessageBox.alert('錯誤', '點收量不得超過揀料量');
                        T2Load();
                        return; 
                    }
                    var data = { item: [] };
                    data.item.push(context.record.data);
                    Ext.Ajax.request({
                        url: '/api/AB0015/UpdateMeDocd',
                        method: reqVal_p,

                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T2Load();
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        },
                        jsonData: data
                    }
                    );
                },
                canceledit: function (editor, context, eOpts) {
                }
            }
        },
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false); T2Form.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            },
            beforeedit: function (plugin, context) {
                if (isCR == 'Y') {
                    return false;
                }
                if (T1F3 === '3' || T1F3 === '4') {
                    return true;
                }
                else {
                    return false;
                }
            }  
        }
    });
    function setFormT2a() {
        //.down('#edit').setDisabled(T2Rec === 0);
        //T2Grid.down('#delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            //if (T1F3 === '3' || T1F3 === '4') {
            //    T2Grid.down('#edit').setDisabled(false);
            //    T2Grid.down('#delete').setDisabled(false);
            //}
            //else {
            //    T2Grid.down('#edit').setDisabled(true);
            //    T2Grid.down('#delete').setDisabled(true);
            //}
            tot_apvqty = 0;
            apvqty = 0;
            appqty = 0;
            if (T2LastRec.get('TOT_APVQTY') == null || T2LastRec.get('TOT_APVQTY') == '') {
                apvqty = Number(0);
            }
            appqty = Number(T2LastRec.get('APPQTY'));
            tot_apvqty = Number(apvqty) + Number(appqty);
            f.findField('TOT_APVQTY').setValue(tot_apvqty);
        }
        else {
            T2Form.getForm().reset();
        }
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
                        height: '30%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
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
            title: '查詢',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1QueryForm]
        }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p5: T1F1,
                Action: 3
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
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P3').setValue(arrP3); //default 4已揀料
    T2Query.getForm().findField('P2').setValue("Y");
});
