Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
        var T1LastRec, T2LastRec;
    var T1Name = "核撥作業";
    var T1LastRec;

    var viewModel = Ext.create('WEBAPP.store.AB.AB0019VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
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
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '單據號碼',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '申請人員代碼',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                maxLength: 8,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '申請部門代碼',
                name: 'P3',
                id: 'P3',
                enforceMaxLength: true,
                maxLength: 6,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '核撥庫房代碼',
                name: 'P5',
                id: 'P5',
                enforceMaxLength: true,
                maxLength: 8,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'textfield',
                fieldLabel: '領用庫房代碼',
                name: 'P6',
                id: 'P6',
                enforceMaxLength: true,
                maxLength: 8,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                items: [
                    { boxLabel: '手動申請', name: 'rb', inputValue: '0601' },
                    { boxLabel: '自動申請', name: 'rb', inputValue: '0600' },
                    { boxLabel: '核撥中', name: 'rb', inputValue: '0602', checked: true },
                    { boxLabel: '已結案', name: 'rb', inputValue: '0699' }
                ]
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
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
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                }
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢條件',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //    }
            //},
            {
                text: '核撥',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
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

                        Ext.MessageBox.confirm('訊息', '確認是否核撥單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0019/UpdateStatusBySp',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:核撥成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
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
            }
        ]
    });
    Ext.getCmp('btnSubmit').setDisabled(true);

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T1Query]
            //},
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
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 120
            },
            {
                text: "申請人員",
                dataIndex: 'APPID',
                width: 150
            },
            {
                xtype: 'hidden',
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 150
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 150
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "領用庫房",
                dataIndex: 'TOWH_NAME',
                width: 150
            },
            {
                text: "核撥庫房",
                dataIndex: 'FRWH_NAME',
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
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        setFormT1a();
                        T2Load();
                    }
                }
            },
            selectionchange: function (model, records) {
                Ext.getCmp('btnSubmit').setDisabled(false);
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                T2Load();
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
                xtype: 'displayfield',
                fieldLabel: '申請單號',
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
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '領用庫房',
                name: 'TOWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '核撥庫房',
                name: 'FRWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }
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
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

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
                fieldLabel: '申請單號',
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
                fieldLabel: '申請數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }
        ]
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
        }

    }

    var T2RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        errorsText: '錯誤訊息',
        dirtyText: '請按更新以修改資料或取消變更',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                //if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                //if (Ext.getCmp('viewMode').disabled) {
                //    return false;  // 取消row editing模式
                //} else {
                //var grid = Ext.ComponentQuery.query('grid#grid_contact');
                //    var grid = Ext.ComponentQuery.query('grid')[2];
                //var col = grid.getView().getHeaderCt().getHeaderAtIndex(4);
                //var cols = grid.getView().getHeaderCt().getGridColumns();
                //Ext.each(cols, function (col) {
                //    if (col.text == "原料記號") {
                //        col.setEditor(null);
                //    }
                //});

                //if (context.column.dataIndex == 'PARAU')
                //col.setEditor(null);
                //}
            },
            edit: function (editor, context, eOpts) {
                //var grid = Ext.ComponentQuery.query('#grid_contact')[0];
                //var store = grid.getView().getStore();

                var data = { item: [] };
                data.item.push(context.record.data);

                //alert(context.record.data['NAME_LAST']);
                Ext.Ajax.request({
                    url: '/api/AB0019/UpdateMeDocd',
                    method: reqVal_p,
                    //params: {
                    //    VID5: context.record.data['VID5']
                    //},
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                        }
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    jsonData: data
                });
            },
            canceledit: function (editor, context, eOpts) {
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [T2RowEditing],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
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
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "核撥數量",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            beforeedit: function (plugin, context) {
                var tmpArray = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray[0] !== '0602') {
                    return false;
                }
            },
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null)
                        setFormT2a();
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

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
    //Ext.getCmp('eastform').collapse();
    Ext.getCmp('eastform').expand();
    //T1Load();
});