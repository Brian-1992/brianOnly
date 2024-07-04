Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.getUrlParam = function (param) {
    var params = Ext.urlDecode(location.search.substring(1));
    return param ? params[param] : params;
};
var po = Ext.getUrlParam('po');

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec;

    var viewModel = Ext.create('WEBAPP.store.BH.BH0002VM');

    var T1Store = viewModel.getStore('WB_REPLY');
    var MmcodeStore = viewModel.getStore('MMCODE');
    MmcodeStore.getProxy().setExtraParam("PO_NO", po);
    MmcodeStore.load();

    function T1Load() {
        T1Store.getProxy().setExtraParam("PO_NO", po);
        T1Store.getProxy().setExtraParam("DNO", T2Query.getForm().findField('P2').getValue());
        T2Tool.moveFirst();
    }

    var mLabelWidth = 90;
    var mWidth = 230;

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            //    {
            //        dock: 'top',
            //        xtype: 'toolbar',
            //        layout: 'fit',
            //        items: [T1Query]
            //    },
            {
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "交貨批次",
                dataIndex: 'DNO',
                width: 80
            },
            {
                text: "預計交貨日",
                dataIndex: 'DELI_DT',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "交貨數量",
                dataIndex: 'INQTY',
                width: 80
            },
            {
                //text: "借貨量",
                //dataIndex: 'BW_SQTY',
                //width: 80,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 150,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 100,
            },
            {
                text: "發票號碼",
                dataIndex: 'INVOICE',
                width: 100,
            },
            {
                text: "發票日期",
                dataIndex: 'INVOICE_DT',
                width: 100,
            },
            {
                text: "條碼編號",
                dataIndex: 'BARCODE',
                width: 100,
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 100,
            },
            {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 100,
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
                    }

                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        Ext.getCmp('btnAdd').setDisabled(false);
                        Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDelete').setDisabled(false);
                        Ext.getCmp('btnSubmit').setDisabled(false);

                        if (T1Set === '')
                            setFormT1a();

                        //T2Load();
                    }

                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();

                if (T1LastRec == null) {
                    Ext.getCmp('btnAdd').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnDelete').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                }
            }
        }
    });

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
                        padding: '0 0 5 0',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '交貨批次',
                                name: 'P2',
                                id: 'P2',
                                //readOnly: true,
                                labelWidth: 60,
                                width: 230,
                                padding: '0 4 0 4',
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    //T2Store.removeAll();
                                    //T2Grid.getSelectionModel().deselectAll();
                                    T1Load();
                                    msglabel('訊息區:');
                                }
                            },
                            {
                                xtype: 'button',
                                text: '交貨新增',
                                handler: function () {
                                    Ext.MessageBox.confirm('訊息', '是否確定新增交貨批次?', function (btn, text) {
                                        if (btn === 'yes') {
                                            Ext.Ajax.request({
                                                url: '/api/BH0002/CreateAll',
                                                method: reqVal_p,
                                                params: {
                                                    PO: po
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    if (data.success) {
                                                        msglabel('訊息區:新增交貨批次成功');
                                                        //T2Store.removeAll();
                                                        T2Grid.getSelectionModel().deselectAll();
                                                        T1Load();
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
                            },
                            {
                                xtype: 'button',
                                id: 'btnAdd',
                                text: '新增',
                                handler: function () {
                                    //T1Form.setVisible(true);
                                    //T2Form.setVisible(false);
                                    T1Set = '/api/BH0002/CreateOne';
                                    setFormT1('I', '新增');
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnUpdate',
                                text: '修改',
                                handler: function () {
                                    T1Set = '/api/BH0002/DetailUpdate';
                                    setFormT1('U', '修改');
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnDelete',
                                text: '刪除',
                                handler: function () {
                                    var selection = T2Grid.getSelection();
                                    if (selection.length) {
                                        let seq = '';
                                        //selection.map(item => {
                                        //    seq += item.get('SEQ') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            seq += item.get('SEQ') + ',';
                                        })
                                        Ext.MessageBox.confirm('刪除', '是否確定刪除?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/BH0002/DetailDelete',
                                                    method: reqVal_p,
                                                    params: {
                                                        //SEQ: T1LastRec.data["SEQ"]
                                                        SEQ: seq
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            msglabel('訊息區:刪除成功');
                                                            //T2Store.removeAll();
                                                            T2Grid.getSelectionModel().deselectAll();
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
                                xtype: 'button',
                                id: 'btnSubmit',
                                text: '確認回傳',
                                handler: function () {
                                    var selection = T2Grid.getSelection();
                                    if (selection.length) {
                                        let seq = '';
                                        //selection.map(item => {
                                        //    seq += item.get('SEQ') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            seq += item.get('SEQ') + ',';
                                        })
                                        Ext.MessageBox.confirm('訊息', '是否確定回傳?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/BH0002/DetailSubmit',
                                                    method: reqVal_p,
                                                    params: {
                                                        SEQ: seq
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            msglabel('訊息區:確認回傳成功');
                                                            T2Grid.getSelectionModel().deselectAll();
                                                            T1Load();
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
                                xtype: 'button',
                                id: 'btnImport',
                                text: '匯入',
                                handler: function () {
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnPrint',
                                text: '列印',
                                handler: function () {
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnDownload',
                                text: '下載範本',
                                handler: function () {
                                }
                            },
                            //{
                            //    xtype: 'button',
                            //    text: '清除',
                            //    handler: function () {
                            //        var f = this.up('form').getForm();
                            //        f.reset();
                            //        f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                            //        msglabel('訊息區:');
                            //    }
                            //}
                        ]
                    },
                    {
                        xtype: 'component',
                        autoEl: {
                            tag: 'span',
                            style: 'margin-left: 10px;color:#194c80',
                            html: '※ [交貨新增] 會複製訂單所有品項資料，再填上相關欄位。<br>※ 條碼編號係指藥品出貨之最小單位包裝之條碼。<br>※ 同院內碼不同批號丶效期，請[新增]輸入成另一筆資料。'
                        }
                    }
                ]
            }
        ]
    });

    var T2Panel = Ext.create('Ext.panel.Panel', {
        xtype: 'container',
        items: [T2Query, T2Grid]
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
                fieldLabel: 'PO_NO',
                name: 'PO_NO',
                xtype: 'hidden'
            },
            {
                fieldLabel: 'SEQ',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                fieldLabel: '交貨批次',
                name: 'DNO',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                enforceMaxLength: true,
                maxLength: 21,
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'datefield',
                fieldLabel: '預計交貨日',
                name: 'DELI_DT',
                //id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'combo',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                store: MmcodeStore,
                displayField: 'MMCODE',
                valueField: 'MMCODE',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                enforceMaxLength: true,
                maxLength: 13,
                submitValue: true,
                readOnly: true
            },
            {
                fieldLabel: '交貨數量',
                name: 'INQTY',
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true
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
                readOnly: true
            },
            {
                xtype: 'datefield',
                fieldLabel: '效期',
                name: 'EXP_DATE',
                regex: /\d{7,7}/,
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true
            },
            {
                fieldLabel: '發票號碼',
                name: 'INVOICE',
                enforceMaxLength: true,
                maxLength: 10,
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'datefield',
                fieldLabel: '發票日期',
                name: 'INVOICE_DT',
                //id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                submitValue: true,
                readOnly: true
            },
            {
                fieldLabel: '條碼編號',
                name: 'BARCODE',
                enforceMaxLength: true,
                maxLength: 50,
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true
            },
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
                                T1Set = '';
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
                            //T1Query.getForm().reset();
                            //var v = action.result.etts[0];
                            //T2Store.removeAll();
                            //T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            break;
                        case "R":
                            T1Load();
                            msglabel('訊息區:資料退回成功');
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
        //setFormT1a();
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        //Tabs.setActiveTab('Form');
        //var currentTab = Tabs.getActiveTab();
        //currentTab.setTitle('填寫');

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.WB_REPLY'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField('DNO').setValue(T1LastRec.data['DNO']);
            f.findField('MMCODE').setValue(T1LastRec.data['MMCODE']);
            f.findField('PO_NO').setValue(T1LastRec.data['PO_NO']);
            //u = f.findField("DNO");
            //u.clearInvalid();
        }
        else {
            //f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            //f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            //f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            //f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

        }
        f.findField('x').setValue(x);
        f.findField('DELI_DT').setReadOnly(false);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('INQTY').setReadOnly(false);
        //f.findField('BW_SQTY').setReadOnly(false);
        f.findField('INVOICE').setReadOnly(false);
        f.findField('INVOICE_DT').setReadOnly(false);
        f.findField('BARCODE').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        //u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            viewport.down('#form').setTitle('瀏覽');

            var f = T1Form.getForm();
            f.findField("SEQ").setValue(T1LastRec.data["SEQ"]);
            f.findField("DNO").setValue(T1LastRec.data["DNO"]);
            f.findField("MMCODE").setValue(T1LastRec.data["MMCODE"]);
            f.findField("DELI_DT").setValue(T1LastRec.data["DELI_DT"]);
            f.findField("LOT_NO").setValue(T1LastRec.data["LOT_NO"]);
            f.findField("EXP_DATE").setValue(T1LastRec.data["EXP_DATE"]);


            f.findField("INQTY").setValue(T1LastRec.data["INQTY"]);
            //f.findField("BW_SQTY").setValue(T1LastRec.data["BW_SQTY"]);
            f.findField("INVOICE").setValue(T1LastRec.data["INVOICE"]);
            f.findField("INVOICE_DT").setValue(T1LastRec.data["INVOICE_DT"]);
            f.findField("BARCODE").setValue(T1LastRec.data["BARCODE"]);
            f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

            //tmpArray = T1LastRec.data["APP_NAME"].split(' ');
            //tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
            //if (session['UserId'] !== tmpArray[0] || tmpArray1[0] !== '0601') {
            //    Ext.getCmp("btnUpdate").setDisabled(true);
            //    Ext.getCmp("btnDel").setDisabled(true);
            //    Ext.getCmp("btnSubmit").setDisabled(true);
            //    Ext.getCmp("btnAdd2").setDisabled(true);
            //}
            //else {
            //    Ext.getCmp("btnUpdate").setDisabled(false);
            //    Ext.getCmp("btnDel").setDisabled(false);
            //    Ext.getCmp("btnSubmit").setDisabled(false);
            //    Ext.getCmp("btnAdd2").setDisabled(false);
            //}

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
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        //height: '90%',
                        items: [T2Panel]
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

    Ext.getCmp('btnAdd').setDisabled(true);
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDelete').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
});