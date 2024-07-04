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
var no = Ext.getUrlParam('no');

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec;

    var viewModel = Ext.create('WEBAPP.store.CE0004VM');
    var T1Store = viewModel.getStore('CHK_MAST');
    function T1Load() {
        T1Store.getProxy().setExtraParam("CHK_NO", no);
        T1Store.load(function () {
            var f = T1Query.getForm();
            f.findField("CHK_WH_NO").setValue(this.getAt(0).get('CHK_WH_NO'));
            f.findField("CHK_YM").setValue(this.getAt(0).get('CHK_YM'));
            f.findField("CHK_PERIOD").setValue(this.getAt(0).get('CHK_PERIOD'));
            f.findField("CHK_TYPE").setValue(this.getAt(0).get('CHK_TYPE'));
            f.findField("CHK_STATUS").setValue(this.getAt(0).get('CHK_STATUS'));
            f.findField("CHK_WH_KIND").setValue(this.getAt(0).get('CHK_WH_KIND'));
            f.findField("CHK_NO").setValue(this.getAt(0).get('CHK_NO'));
        });
    }

    var T2Store = viewModel.getStore('CHK_DETAIL');
    function T2Load() {
        T2Store.getProxy().setExtraParam("CHK_NO", no);
        T1Tool.moveFirst();
    }

    //var MmcodeStore = viewModel.getStore('MMCODE');
    //MmcodeStore.getProxy().setExtraParam("PO_NO", po);
    //MmcodeStore.load();

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        id: 'aa',
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
                xtype: 'panel',
                border: false,
                layout: {
                    type: 'table',
                    columns: 5
                },
                padding: '0 0 5 0',
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '庫房代碼',
                        name: 'CHK_WH_NO',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點年月',
                        name: 'CHK_YM',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點期',
                        name: 'CHK_PERIOD',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點類別',
                        name: 'CHK_TYPE',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'CHK_STATUS',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '庫房分類',
                        name: 'CHK_WH_KIND',
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤點單號',
                        name: 'CHK_NO',
                        width: 200,
                        padding: '0 4 0 4'
                    },
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '盤點單負責人',
                    //    value: session['UserName'],
                    //    width: 170,
                    //    padding: '0 4 0 4'
                    //}

                ]
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    T1Set = '/api/CE0004/DetailUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '完成盤點調整',
                id: 'btnFinish',
                name: 'btnFinish',
                handler: function () {
                    //T1Set = '/api/CE0004/DetailUpdate';
                    //setFormT1('U', '修改');
                    Ext.MessageBox.confirm('訊息', '確認完成盤點調整?', function (btn, text) {
                        if (btn === 'yes') {
                            var f = T1Query.getForm();
                            Ext.Ajax.request({
                                url: '/api/CE0004/Finish',
                                method: reqVal_p,
                                params: {
                                    CHK_NO: no,
                                    CHK_YM: f.findField('CHK_YM').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //T2Store.removeAll();
                                        //T1Grid.getSelectionModel().deselectAll();
                                        //T1Load();
                                        //Ext.getCmp('btnSubmit').setDisabled(true);
                                        
                                        var tp = window.parent.tabs;
                                        //var activeTab = tp.getActiveTab();
                                        //var activeTabIndex = tp.items.findIndex('id', activeTab.id);
                                        //tp.remove(activeTabIndex);

                                        // get the active panel
                                        //var panel = parent.layout.getRegion('center').getActivePanel();
                                        // remove active panel
                                        //parent.layout.getRegion('center').remove(panel);
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
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
                text: '過帳與結案',
                id: 'btnEnd',
                name: 'btnEnd',
                handler: function () {

                }
            },
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
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
                items: [T1Tool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
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
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: 130,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
            },
            {
                text: "儲位名稱",
                dataIndex: 'LOC_NAME',
                width: 130,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
            },
            {
                text: "計量單位",
                dataIndex: 'M_PURUN',
                width: 100,
            },
            {
                text: "電腦量",
                dataIndex: 'STORE_QTYC',
                width: 70,
                align: 'right'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 70,
                align: 'right'
            },
            //{
            //    text: "狀態",
            //    dataIndex: 'STATUS_INI',
            //    width: 80,
            //},
            {
                text: "附註",
                dataIndex: 'CHK_REMARK',
                width: 300,
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
                        //Ext.getCmp('btnAdd').setDisabled(false);
                        Ext.getCmp('btnUpdate').setDisabled(false);
                        //Ext.getCmp('btnDelete').setDisabled(false);
                        //Ext.getCmp('btnSubmit').setDisabled(false);

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
                    //Ext.getCmp('btnAdd').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    //Ext.getCmp('btnDelete').setDisabled(true);
                    //Ext.getCmp('btnSubmit').setDisabled(true);
                }
            }
        }
    });

    var T1Panel = Ext.create('Ext.panel.Panel', {
        xtype: 'container',
        items: [T1Query, T1Grid]
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
                fieldLabel: 'CHK_NO',
                name: 'CHK_NO',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '儲位',
                name: 'STORE_LOC',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '儲位名稱',
                name: 'LOC_NAME',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'M_PURUN',
                submitValue: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '電腦量',
                name: 'STORE_QTYC',
                submitValue: true,
            },
            {
                fieldLabel: '盤點量',
                name: 'CHK_QTY',
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                submitValue: true,
                readOnly: true
            },
            //{
            //    fieldLabel: '條碼編號',
            //    name: 'BARCODE',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    submitValue: true,
            //    readOnly: true
            //},
            {
                xtype: 'textareafield',
                fieldLabel: '附註',
                name: 'CHK_REMARK',
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
                            T2Load();
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

        f.findField('x').setValue(x);
        f.findField('CHK_NO').setValue(T1Query.getForm().findField('CHK_NO').getValue());
        f.findField('CHK_QTY').setReadOnly(false);
        f.findField('CHK_REMARK').setReadOnly(false);

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
            f.findField("MMCODE").setValue(T1LastRec.data["MMCODE"]);
            f.findField("MMNAME_C").setValue(T1LastRec.data["MMNAME_C"]);
            f.findField("MMNAME_E").setValue(T1LastRec.data["MMNAME_E"]);
            f.findField("STORE_LOC").setValue(T1LastRec.data["STORE_LOC"]);
            f.findField("LOC_NAME").setValue(T1LastRec.data["LOC_NAME"]);
            f.findField("M_PURUN").setValue(T1LastRec.data["M_PURUN"]);
            f.findField("STORE_QTYC").setValue(T1LastRec.data["STORE_QTYC"]);
            f.findField("CHK_QTY").setValue(T1LastRec.data["CHK_QTY"]);
            f.findField("CHK_REMARK").setValue(T1LastRec.data["CHK_REMARK"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
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
                        items: [T1Panel]
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

    T1Load();
    T2Load();
});