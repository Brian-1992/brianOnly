Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BA0005/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材非庫備品臨時申請單申購彙總";

    var T1Rec = 0;
    var T1LastRec = null;

    var GetCLSNAME = '../../../api/BA0005/GetCLSNAME';

    var CLSNAMEStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //var form_recStatus = Ext.create('Ext.data.Store', {
    //    fields: ['KEYCODE', 'VALUE'],
    //    data: [
    //        { "KEYCODE": "A", "NAME": "啟用", "COMBITEM": "A 啟用" },
    //        { "KEYCODE": "X", "NAME": "停用", "COMBITEM": "X 停用" }
    //    ]
    //});

    function SetCLSNAME() { //建立物料分類的下拉式選單
        Ext.Ajax.request({
            url: GetCLSNAME,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var clsnames = data.etts;
                    if (clsnames.length > 0) {
                        //CLSNAMEStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < clsnames.length; i++) {
                            CLSNAMEStore.add({ VALUE: clsnames[i].VALUE, TEXT: clsnames[i].TEXT });
                        }
                        T1Query.getForm().findField("P0").setValue(clsnames[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

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
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P0',
                id: 'P0',
                fieldCls: 'required',
                allowBlank: 'false',
                store: CLSNAMEStore,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true,
            }, {
                xtype: 'datefield',
                id: 'P1',
                name: 'P1',
                fieldLabel: '單位申請日期',
                fieldCls: 'required',
                allowBlank: 'false',
                width: 200,
                value: new Date(),
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                        T1Query.getForm().findField('P2').setMinValue(nVal);
                    }
                }
            }, {
                xtype: 'datefield',
                id: 'P2',
                name: 'P2',
                fieldLabel: '至',
                fieldCls: 'required',
                allowBlank: 'false',
                width: 130,
                labelWidth: 20,
                value: new Date(),
                listeners: {
                    change: function (field, nVal, oVal, eOpts) {
                        T1Query.getForm().findField('P1').setMaxValue(nVal);
                    }
                }
            },
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                    if (T1Query.getForm().findField('P0').getValue() == null || T1Query.getForm().findField('P0').getValue() == '' ||
                        T1Query.getForm().findField('P1').getValue() == null || T1Query.getForm().findField('P2').getValue() == null) {
                        Ext.Msg.alert('提醒', '物料分類與日期不可空白！');
                    }
                    else {
                        T1Load();
                    }
                    msglabel('訊息區:');
                }
            },
            {
                xtype: 'hidden',
                //xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                    msglabel('訊息區:');
                }
            },
            //{
            //    xtype: 'button',
            //    text: '彙總並產生申購單',
            //    handler: function () {
            //        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
            //        //T1Load();
            //        msglabel('訊息區:');
            //    }
            //}
                {
                    xtype: 'button',
                    text: '彙總並產生申購單',
                    handler: function () {                        
                            Ext.MessageBox.confirm('提示', '確定要產生申購單？', callBackFunc);
                            function callBackFunc(id) {
                                if (id == 'yes') {
                                    Ext.Ajax.request({
                                        url: '/api/BA0005/CreateOrder',
                                        method: reqVal_p,
                                        params: {                                            
                                            p0: T1Query.getForm().findField('P0').getValue(),
                                            p1: T1Query.getForm().findField('P1').rawValue,
                                            p2: T1Query.getForm().findField('P2').rawValue,                                            
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                //T1Query.getForm().findField('x').setValue("order");
                                                T1Load();
                                                //Ext.MessageBox.alert('提示', T1Query.getForm().findField('P0').getDisplayValue() + '類申購單彙總成功!!');
                                                Ext.MessageBox.alert('提示', data.msg);
                                                //msglabel('訊息區:產生申購單');
                                            }
                                            else
                                                Ext.MessageBox.alert('提示', data.msg);
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                            };                        
                    }
                }
            ]
        },]
    });

    var T1Store = Ext.create('WEBAPP.store.BA0005VM', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),                    
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    //p4: T1Query.getForm().findField('P4').getValue()
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
            //{
            //    text: '新增', handler: function () {
            //        T1Set = '/api/BA0005/Create'; // BA0005Controller的Create
            //        msglabel('訊息區:');
            //        setFormT1('I', '新增');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/BA0005/Update';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/BA0005/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
        ]
    });

    //function setFormT1(x, t) {
    //    viewport.down('#t1Grid').mask();
    //    viewport.down('#form').setTitle(t + T1Name);
    //    viewport.down('#form').expand();
    //    var f = T1Form.getForm();
    //    if (x === "I") {
    //        isNew = true;
    //        var r = Ext.create('WEBAPP.model.PhVender'); // /Scripts/app/model/PhVender.js
    //        T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
    //        u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
    //        u.setReadOnly(false);
    //        u.clearInvalid();
    //        f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
    //    }
    //    else {
    //        u = f.findField('AGEN_NAMEC');
    //    }
    //    f.findField('x').setValue(x);
    //    f.findField('AGEN_NAMEC').setReadOnly(false);
    //    f.findField('AGEN_NAMEE').setReadOnly(false);
    //    f.findField('AGEN_ADD').setReadOnly(false);
    //    f.findField('AGEN_FAX').setReadOnly(false);
    //    f.findField('AGEN_TEL').setReadOnly(false);
    //    f.findField('AGEN_ACC').setReadOnly(false);
    //    f.findField('UNI_NO').setReadOnly(false);
    //    f.findField('AGEN_BOSS').setReadOnly(false);
    //    f.findField('REC_STATUS').setReadOnly(false);
    //    f.findField('EMAIL').setReadOnly(false);
    //    f.findField('EMAIL_1').setReadOnly(false);
    //    f.findField('AGEN_BANK').setReadOnly(false);
    //    f.findField('AGEN_SUB').setReadOnly(false);
    //    T1Form.down('#cancel').setVisible(true);
    //    T1Form.down('#submit').setVisible(true);
    //    u.focus();
    //}

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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "責任中心",
            dataIndex: 'APPDEPT',
            width: 100
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 230
        }, {
            text: "物料類別",
            dataIndex: 'MAT_CLASS',
            width: 200
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "申請量",
            dataIndex: 'APPQTY',
            width: 60
        }, {
            text: "狀態",
            dataIndex: 'FLOWID',
            width: 300
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                //Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //setFormT1a();
            }
        }
    });
    //function setFormT1a() {
    //    T1Grid.down('#edit').setDisabled(T1Rec === 0);
    //    // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
    //    if (T1LastRec) {
    //        isNew = false;
    //        T1Form.loadRecord(T1LastRec);
    //        var f = T1Form.getForm();
    //        f.findField('x').setValue('U');
    //        var u = f.findField('AGEN_NO');
    //        u.setReadOnly(true);
    //        u.setFieldStyle('border: 0px');
    //        if (T1LastRec.data['REC_STATUS'] == 'X')
    //            T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
    //    }
    //    else {
    //        T1Form.getForm().reset();
    //    }
    //}

    // 顯示明細/新增/修改輸入欄
    //var T1Form = Ext.widget({
    //    xtype: 'form',
    //    layout: 'form',
    //    frame: false,
    //    cls: 'T1b',
    //    title: '',
    //    autoScroll: true,
    //    bodyPadding: '5 5 0',
    //    fieldDefaults: {
    //        labelAlign: 'right',
    //        msgTarget: 'side',
    //        labelWidth: 90
    //    },
    //    defaultType: 'textfield',
    //    items: [{
    //        fieldLabel: 'Update',
    //        name: 'x',
    //        xtype: 'hidden'
    //    }, {
    //        fieldLabel: '廠商碼',
    //        name: 'AGEN_NO',
    //        enforceMaxLength: true,
    //        maxLength: 6,
    //        regexText: '只能輸入英文字母與數字',
    //        regex: /^[\w]{0,6}$/, // 用正規表示式限制可輸入內容
    //        readOnly: true,
    //        allowBlank: false, // 欄位為必填
    //        fieldCls: 'required'
    //    }, {
    //        fieldLabel: '廠商中文名稱',
    //        name: 'AGEN_NAMEC',
    //        enforceMaxLength: true,
    //        maxLength: 100,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '廠商英文名稱',
    //        name: 'AGEN_NAMEE',
    //        enforceMaxLength: true,
    //        maxLength: 100,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '廠商住址',
    //        name: 'AGEN_ADD',
    //        enforceMaxLength: true,
    //        maxLength: 100,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '傳真號碼',
    //        name: 'AGEN_FAX',
    //        enforceMaxLength: true,
    //        maxLength: 14,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '電話號碼',
    //        name: 'AGEN_TEL',
    //        enforceMaxLength: true,
    //        maxLength: 60,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '銀行帳號',
    //        name: 'AGEN_ACC',
    //        enforceMaxLength: true,
    //        maxLength: 15,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '統一編號',
    //        name: 'UNI_NO',
    //        enforceMaxLength: true,
    //        maxLength: 9,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '負責人姓名',
    //        name: 'AGEN_BOSS',
    //        enforceMaxLength: true,
    //        maxLength: 60,
    //        readOnly: true
    //    }, {
    //        xtype: 'combo',
    //        fieldLabel: '修改狀態碼',
    //        name: 'REC_STATUS',
    //        store: form_recStatus,
    //        queryMode: 'local',
    //        displayField: 'COMBITEM',
    //        valueField: 'KEYCODE',
    //        editable: false,
    //        forceSelection: true,
    //        readOnly: true
    //    }, {
    //        fieldLabel: 'EMAIL',
    //        name: 'EMAIL',
    //        enforceMaxLength: true,
    //        maxLength: 60,
    //        readOnly: true
    //    }, {
    //        fieldLabel: 'EMAIL_1',
    //        name: 'EMAIL_1',
    //        enforceMaxLength: true,
    //        maxLength: 60,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '銀行代號',
    //        name: 'AGEN_BANK',
    //        enforceMaxLength: true,
    //        maxLength: 3,
    //        readOnly: true
    //    }, {
    //        fieldLabel: '銀行分行別',
    //        name: 'AGEN_SUB',
    //        enforceMaxLength: true,
    //        maxLength: 4,
    //        readOnly: true
    //    }],
    //    buttons: [{
    //        itemId: 'submit', text: '儲存', hidden: true,
    //        handler: function () {
    //            if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
    //                if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
    //                    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
    //                    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
    //                else {
    //                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
    //                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
    //                        if (btn === 'yes') {
    //                            T1Submit();
    //                        }
    //                    }
    //                    );
    //                }
    //            }
    //            else {
    //                Ext.Msg.alert('提醒', '輸入資料格式有誤');
    //                msglabel('訊息區:輸入資料格式有誤');
    //            }
    //        }
    //    }, {
    //        itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
    //    }]
    //});
    //function T1Submit() {
    //    var f = T1Form.getForm();
    //    if (f.isValid()) {
    //        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    //        myMask.show();
    //        f.submit({
    //            url: T1Set,
    //            success: function (form, action) {
    //                myMask.hide();
    //                var f2 = T1Form.getForm();
    //                var r = f2.getRecord();
    //                switch (f2.findField("x").getValue()) {
    //                    case "I":
    //                        // 新增後,將key代入查詢條件,只顯示剛新增的資料
    //                        var v = action.result.etts[0];
    //                        T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
    //                        T1Load();
    //                        msglabel('訊息區:資料新增成功');
    //                        break;
    //                    case "U":
    //                        var v = action.result.etts[0];
    //                        r.set(v);
    //                        r.commit();
    //                        msglabel('訊息區:資料修改成功');
    //                        break;
    //                    //case "D":
    //                    //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
    //                    //    r.commit();
    //                    //    break;
    //                }
    //                T1Cleanup();
    //            },
    //            failure: function (form, action) {
    //                myMask.hide();
    //                switch (action.failureType) {
    //                    case Ext.form.action.Action.CLIENT_INVALID:
    //                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
    //                        break;
    //                    case Ext.form.action.Action.CONNECT_FAILURE:
    //                        Ext.Msg.alert('失敗', 'Ajax communication failed');
    //                        break;
    //                    case Ext.form.action.Action.SERVER_INVALID:
    //                        Ext.Msg.alert('失敗', action.result.msg);
    //                        break;
    //                }
    //            }
    //        });
    //    }
    //}
    //function T1Cleanup() {
    //    viewport.down('#t1Grid').unmask();
    //    var f = T1Form.getForm();
    //    f.reset();
    //    f.getFields().each(function (fc) {
    //        if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
    //            fc.setReadOnly(true);
    //        } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
    //            fc.readOnly = true;
    //        }
    //    });
    //    T1Form.down('#cancel').hide();
    //    T1Form.down('#submit').hide();
    //    viewport.down('#form').setTitle('瀏覽');
    //    Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
    //    setFormT1a();
    //}


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
            items: [T1Grid]
        },
            //{
            //    itemId: 'form',
            //    region: 'east',
            //    collapsible: true,
            //    floatable: true,
            //    width: 300,
            //    title: '瀏覽',
            //    border: false,
            //    collapsed: true,
            //    layout: {
            //        type: 'fit',
            //        padding: 5,
            //        align: 'stretch'
            //    },
            //    items: [T1Form]
            //}
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    SetCLSNAME();
});
