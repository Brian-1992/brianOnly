Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T3Set = '/api/AA0035/MeDoceUpdate';
    var WhnoComboGet = '../../../api/AA0035/GetWhnoCombo';
    var GetMmdataByMmcode = '/api/AA0035/GetMmdataByMmcode';
    var GetMeDocdMaxSeq = '/api/AA0035/GetMeDocdMaxSeq';
    var T1LastRec = null, T2LastRec = null; T3LastRec = null;
    var T1Name = '過效期報廢';
    
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName']; 
    //var userId, userName, userInid, userInidName;
    var tempT1Record = null;
    var expandExtForm = true;

    var viewModel = Ext.create('WEBAPP.store.AA.AA0035VM');

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        whnoQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1Store = viewModel.getStore('ME_DOCM');
    T1Store.on('load', function (store, options) {
        
        var index = -1;
        if (tempT1Record != null) {
            for (var i = 0; i < store.data.items.length; i++) {
                if (tempT1Record.data.DOCNO == store.data.items[i].get('DOCNO')) {
                    index = i;
                }
            }
        }
        if (index > -1) {
            T1Grid.getSelectionModel().select(index, true);
            if (store.data.items[index].get('LIST_ID') == 'Y') {
                Ext.getCmp('exp').enable();
            } else {
                Ext.getCmp('exp').disable();
            }
            T1LastRec = Object.assign({}, tempT1Record);
            tempT1Record = null;

            Ext.getCmp('btnUpdate').setDisabled(false);
            Ext.getCmp('btnDel').setDisabled(false);

            Ext.getCmp('btnUpdate2').setDisabled(true);
            Ext.getCmp('btnDel2').setDisabled(true);
            Ext.getCmp('btnAdd2').setDisabled(false);
            Ext.getCmp('btnSubmit').setDisabled(false);
        }
    });
    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {

           // Ext.getCmp('btnUpdate2').setDisabled(true);
           // Ext.getCmp('btnDel2').setDisabled(true);
           // Ext.getCmp('btnAdd2').setDisabled(false);

            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.getProxy().setExtraParam("p1", T1LastRec.data["FRWH"]);
            T2Tool.moveFirst();

        }
    }

    var T3Store = viewModel.getStore('ME_DOCE');
    function T3Load() {
        if (T2LastRec != null && T2LastRec.data["DOCNO"] !== '') {
            
            Ext.getCmp('btnUpdate3').setDisabled(false);
            Ext.getCmp('btnCancel3').setDisabled(false);

            T3Store.getProxy().setExtraParam("p0", T2LastRec.data["DOCNO"]);
            T3Store.getProxy().setExtraParam("p1", T2LastRec.data["SEQ"]);
            T3Tool.moveFirst();
        }
    }

    // 查詢欄位
    var mLabelWidth = 70;
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
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '異動單號',
                            name: 'P0',
                            id: 'P0',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4'
                        },
                        {
                            xtype: 'datefield',
                                fieldLabel: '調帳日期',
                                name: 'P1',
                                id: 'P1',
                                labelWidth: mLabelWidth,
                                width:150,
                                padding: '0 4 0 4',
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 8,
                            width: 88,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                        },
                        //{
                        //    xtype: 'textfield',
                        //    fieldLabel: '調帳日期',
                        //    name: 'P1',
                        //    id: 'P1',
                        //    enforceMaxLength: true,
                        //    maxLength: 7,
                        //    regexText: '請填入民國年月日',
                        //    regex: /\d{7,7}/,
                        //    labelWidth: mLabelWidth,
                        //    width: 130,
                        //    padding: '0 4 0 4',
                        //}, {
                        //    xtype: 'textfield',
                        //    fieldLabel: '至',
                        //    name: 'P2',
                        //    id: 'P2',
                        //    enforceMaxLength: true,
                        //    maxLength: 7,
                        //    regexText: '請填入民國年月日',
                        //    regex: /\d{7,7}/,
                        //    labelSeparator: '',
                        //    width: 68,
                        //    labelWidth: 8,
                        //    padding: '0 4 0 4'
                        //},
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P3',
                            id: 'P3',
                            fieldLabel: '出庫庫房',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: true,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                T1Load();
                                Ext.getCmp('btnUpdate').setDisabled(true);
                                Ext.getCmp('btnDel').setDisabled(true);
                                Ext.getCmp('btnSubmit').setDisabled(true);
                                //Ext.getCmp('btnAdd2').setDisabled(true);

                                //Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    //var r = WEBAPP.model.ME_DOCM.create({
                    //    //APPNAME: 'aaa'
                    //});
                    //T1Store.add(r.copy());
                    setFormVisible(true, false);
                    T1Set = '/api/AA0035/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    setFormVisible(true, false);
                    T1Set = '/api/AA0035/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
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

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0035/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                                            msglabel('訊息區:資料刪除成功');
                                            //T2Store.removeAll();

                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
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
                }
            },
            //{
            //    text: '過帳',
            //    id: 'btnSubmit',
            //    name: 'btnSubmit',
            //    disabled:true,
            //    handler: function () {

            //    }
            //}
            {
                text: '過帳',
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

                        Ext.MessageBox.confirm('訊息', '確認是否結案單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AA0035/UpdateStatusBySp',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        
                                        if (data.success) {
                                            msglabel('過帳成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
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
                id: 'exp',
                disabled: true,
                //hidden: true,
                handler: function () {
                    showExpWindow(T1LastRec.data.DOCNO, 'O', viewport);
                }
            },
            {
                itemId: 'print', id: 'print', text: '列印', handler: function () {
                    showWin3();
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
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout:'fit',
                items: [T1Query]
            },
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
                text: "異動單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 110,
                xtype: 'templatecolumn',
                tpl: '{FLOWID} {FLOWID_N}'
            },
            {
                text: "出庫庫房",
                dataIndex: 'FRWH_N',
                width: 130
            },
            {
                text: "調帳日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "申請人員",
                dataIndex: 'CREATE_USER_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {

                T2Store.removeAll();
                T3Store.removeAll();
                
                msglabel('訊息區:');
                Ext.getCmp('eastform').collapse();
                
                if (Ext.getCmp('eastform').collapsed == true && expandExtForm == true) {
                    setFormVisible(true, false);
                    Ext.getCmp('eastform').expand();
                }
                
                T2Grid.getSelectionModel().deselectAll();

                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                T2Load();

                expandExtForm = true;
            },
            itemclick: function (self, record, item, index, e, eOpts) {
                setFormVisible(true, false);
                T2Grid.getSelectionModel().deselectAll();

                if ((T1LastRec != null && userId.trim() == T1LastRec.data.CREATE_USER.trim()) && T1LastRec.data.FLOWID != '0599') {
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    Ext.getCmp('btnDel').setDisabled(false);
                    Ext.getCmp('btnSubmit').setDisabled(false);

                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(false);

                    Ext.getCmp('btnUpdate3').setDisabled(true);
                    Ext.getCmp('btnCancel3').setDisabled(true);
                } else {
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);

                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);

                    Ext.getCmp('btnUpdate3').setDisabled(true);
                    Ext.getCmp('btnCancel3').setDisabled(true);
                }

                if (T1LastRec != null && T1LastRec.data.LIST_ID == 'Y') {
                    Ext.getCmp('exp').setDisabled(false);
                } else {
                    Ext.getCmp('exp').setDisabled(true);
                }
                tempT1Record = Object.assign({}, record);
                T2Load();
            }
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {
                    setFormVisible(false, true);
                    T1Set = '/api/AA0035/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                handler: function () {
                    setFormVisible(false, true);
                    //T1Query.setVisible(false);
                    T1Set = '/api/AA0035/DetailUpdate';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('SEQ') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

                        tempT1Record = Object.assign({}, T1LastRec);

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0035/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            T2Load();
                                            Ext.getCmp('eastform').collapse();

                                            Ext.getCmp('btnUpdate2').disable();
                                            Ext.getCmp('btnDel2').disable();
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
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
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
            //{
            //    text: "項次",
            //    dataIndex: 'SEQ',
            //    width: 60,
            //    align: 'right',
            //},
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                //renderer: function (val, meta, record) {
                    
                //    var mmcode = record.data.MMCODE;
                //    var param3 = record.data.DOCNO;
                //    return '<a href=javascript:void(0)>' + mmcode + '</a>';
                //},
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
                text: "數量",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "金額",
                dataIndex: 'AMOUNT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "結存量",
                dataIndex: 'AMOUNT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 100
            },
            //{
            //    text: "核撥數量",
            //    dataIndex: 'APVQTY',
            //    width: 70,
            //    editor: {
            //        xtype: 'textfield',
            //        regexText: '只能輸入數字',
            //        regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            //    }
            //},
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            //cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
            //    if (cellIndex != 2) {
            //        return;
            //    }

            //    Ext.getCmp('eastform').expand();
            //    msglabel('訊息區:');

            //    T2LastRec = record;

            //    T3Store.removeAll();

            //    window.show();
                
            //    T3Load();
            //},
            itemclick: function (self, record, item, index, e, eOpts) {
                

                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T2LastRec = record;
                setFormT2a();

                setFormVisible(false, true);

                Ext.getCmp('eastform').expand();
                
                if ((T2LastRec != null && userId.trim() == T1LastRec.data.CREATE_USER.trim()) && T1LastRec.data.FLOWID != '0599') {
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);

                    Ext.getCmp('btnUpdate2').setDisabled(false);
                    Ext.getCmp('btnDel2').setDisabled(false);
                    Ext.getCmp('btnAdd2').setDisabled(false);

                    Ext.getCmp('btnUpdate3').setDisabled(false);
                    Ext.getCmp('btnCancel3').setDisabled(false);
                } else {
                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);

                    Ext.getCmp('btnUpdate3').setDisabled(true);
                    Ext.getCmp('btnCancel3').setDisabled(true);
                }
            }
        }
    });

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '更新',
                id: 'btnUpdate3',
                name: 'btnUpdate3',
                disabled: true,
                handler: function () {
                    var apvqty = Number(T2LastRec.data.APVQTY);

                    var tempData = T3Grid.getStore().data.items;
                    var doceQty = 0;
                    for (var i = 0; i < tempData.length; i++) {
                        doceQty += Number(tempData[i].data.APVQTY)
                    }
                    if (apvqty != doceQty) {
                        var msg = '<span style="color: red">細項總數量(' + doceQty + ')與申請量(' + apvqty + ')不符合</span>，請重新輸入';
                        Ext.Msg.alert('提示', msg );
                        return;
                    }


                    var data = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }


                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    Ext.Ajax.request({
                        url: T3Set,
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            myMask.hide();
                            if (data.success) {

                                msglabel('訊息區:資料修改成功');
                                T3Tool.moveFirst();
                            } else {
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }, {
                text: '取消',
                id: 'btnCancel3',
                name: 'btnCancel3',
                disabled: true,
                handler: function () {
                    T3Store.load({
                        params: {
                            start: 0
                        }
                    });
                }
            }
        ]
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [
            //{
            //    text: "單據號碼",
            //    dataIndex: 'DOCNO',
            //},
            //{
            //    text: "項次",
            //    dataIndex: 'SEQ',
            //    width: 60,
            //},
            {
                xtype:'datecolumn',
                text: "有效日期",
                dataIndex: 'EXPDATE',
                width: 100,
                format: 'Xmd',
                editor: {
                    xtype: 'datefield',
                    allowBlank: false,
                    format: 'Xmd',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    }
                }
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "<span style='color:red'>數量</span>",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                editor: {
                    xtype: 'numberfield',
                    minValue: 0,
                    hideTrigger: true
                }
            },
            {
                text: "警示效期(年月)",
                dataIndex: 'WARNYM',
                width: 110
            },
            {
                text: "人員",
                dataIndex: 'UPDATE_USER_NAME',
                width: 130,
            },
            {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                msglabel('訊息區:');
                Ext.getCmp('eastform').expand();

                //var f = T2Form.getForm().findField('T3Grid');
                //f.show();

                //T3Rec = records.length;
                //T3LastRec = records[0];
                //setFormT3a();
            }
        }
    });

    //Date.prototype.yyymmdd = function () {
    //    var mm = this.getMonth() + 1; // getMonth() is zero-based
    //    var dd = this.getDate();

    //    return [this.getFullYear() - 1911,
    //    (mm > 9 ? '' : '0') + mm,
    //    (dd > 9 ? '' : '0') + dd
    //    ].join('');
    //};

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("FRWH"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField("FLOWID_N").setValue("新增");
            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A

            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);

            f.findField("APPTIME").setValue('');
        }
        else {
            u = f.findField('FRWH');

            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("FLOWID_N").setValue(T1LastRec.data["FLOWID_N"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("CREATE_USER_NAME").setValue(T1LastRec.data["CREATE_USER_NAME"]);
            //f.findField('APPTIME').setReadOnly(false);

        }
        f.findField('x').setValue(x);
        f.findField('FRWH').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FLOWID_N").setValue(T1LastRec.data["FLOWID_N"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("CREATE_USER_NAME").setValue(T1LastRec.data["CREATE_USER_NAME"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

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
                fieldLabel: '異動單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請部門',
                name: 'INID_NAME',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '狀態',
                name: 'FLOWID_N',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '出庫庫房',
                name: 'FRWH',
                enforceMaxLength: true,
                readOnly: true,
                store: whnoQueryStore,
                displayField: 'TEXT',
                valueField: 'VALUE',
                readOnly: true,
                allowBlank: false,
                queryMode: 'local',
                anyMatch: true,
                allowBlank: true,
            },
            {
                xtype: 'displayfield',
                fieldLabel: '調帳日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'CREATE_USER_NAME',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

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
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var v = action.result.etts[0];
                    T1Query.getForm().findField('P0').setValue(v.DOCNO);

                    T1Load();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            //Ext.Msg.alert('訊息', '新增成功');
                            break;
                        case "U":
                            
                            msglabel('訊息區:資料更新成功');
                            //Ext.Msg.alert('訊息', '更新成功');
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
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
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
    function setFormT2(x, t) {
        
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        
        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH"]);
            f.findField("WH_NO").setValue(T1LastRec.data["FRWH"]);
            f.findField("MMCODE").show();
            f.findField("MMCODE_DISPLAY").hide();
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').show();

            var promise = meDocdSeqPromise(f.findField("DOCNO2").getValue());
            promise.then(function (success) {
                var data = JSON.parse(success);
                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        var seq = list[0].SEQ;
                        f.findField("SEQ").setValue(seq);
                    }
                }
            });

            //T3Load();
        }
        else {

            f.findField("MMCODE").hide();
            f.findField("MMCODE_DISPLAY").show();

            u = f.findField('APVQTY');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_N"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField("MMCODE_DISPLAY").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();

            f.findField("WH_NO").setValue(T1LastRec.data["FRWH"]);
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
        }
        f.findField('x').setValue(x);
        //f.findField('MMCODE').setReadOnly(false);
        f.findField('APVQTY').setReadOnly(false);
        f.findField('APLYITEM_NOTE').setReadOnly(false);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }
    function meDocdSeqPromise(docno) {
        var deferred = new Ext.Deferred();

        Ext.Ajax.request({
            url: GetMeDocdMaxSeq,
            method: reqVal_p,
            params: {
                DOCNO: docno
            },
            success: function (response) {
                deferred.resolve(response.responseText);
            },

            failure: function (response) {
                deferred.reject(response.status);
            }
        });

        return deferred.promise; //will return the underlying promise of deferred

    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            //f.findField("FRWH2").setValue(T1LastRec.data["FRWH_N"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);

            f.findField("MMCODE").show();
            f.findField("MMCODE_DISPLAY").hide();
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').show();

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APVQTY').setReadOnly(true);
            f.findField('APLYITEM_NOTE').setReadOnly(true);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        labelWidth: 90,
        labelAlign: 'right',
        width: 200,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0035/GetMMCodeCombo',

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                
                tmpArray = f.findField("FRWH2").getValue().split(' ');
                return {
                    //MMCODE: f.findField("MMCODE").getValue(),
                    WH_NO: tmpArray[0]
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                }
            }
        }
    });

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
        defaultType: 'textfield',
        items: [
            {
                xtype: 'container',
                items: [
                    {
                        fieldLabel: 'Update',
                        name: 'x',
                        xtype: 'hidden'
                    },
                    {
                        xtype: 'hidden',
                        name: 'DOCNO',

                    },
                    {
                        xtype: 'hidden',
                        name: 'WH_NO',

                    },
                    {
                        xtype: 'hidden',
                        name: 'SEQ',

                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '異動單號',
                        name: 'DOCNO2',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '出庫庫房',
                        name: 'FRWH2',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        hidden: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '項次',
                    //    name: 'SEQ',
                    //    enforceMaxLength: true,
                    //    readOnly: true,
                    //    submitValue: true,
                    //    labelWidth: 90,
                    //    labelAlign: 'right'
                    //},
                    //{
                    //    xtype: 'textfield',
                    //    fieldLabel: '院內碼',
                    //    name: 'MMCODE',
                    //    enforceMaxLength: true,
                    //    maxLength: 13,
                    //    submitValue: true,
                    //    allowBlank: false,
                    //    fieldCls: 'required',
                    //    labelWidth: 90,
                    //    labelAlign: 'right',
                    //    listeners: {
                    //        blur: function (field, eve, eOpts) {
                    //            if (T2Form.getForm().findField('x').getValue() == "I") {
                    //                if (field.getValue() != '')
                    //                    chkMMCODE(field.getValue());
                    //                //T3Load();
                    //            }

                    //        }
                    //    }
                    //},
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        padding: '0 7 7 0',
                        id:'mmcodeComboSet',
                        items: [
                            mmCodeCombo,
                            {
                                xtype: 'button',
                                itemId: 'btnMmcode',
                                iconCls: 'TRASearch',
                                handler: function () {
                                    var f = T2Form.getForm();
                                    if (!f.findField("MMCODE").readOnly) {
                                        tmpArray = f.findField("FRWH2").getValue().split(' ');
                                        popMmcodeForm(viewport, '/api/AB0010/GetMmcode', { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0] }, setMmcode);
                                    }
                                }
                            },

                        ]
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE_DISPLAY',
                        labelWidth: 90,
                        labelAlign: 'right',
                        hidden: true
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        enforceMaxLength: true,
                        readOnly: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        enforceMaxLength: true,
                        readOnly: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '數量',
                        name: 'APVQTY',
                        enforceMaxLength: true,
                        submitValue: true,
                        fieldCls: 'required',
                        hideTrigger: true,
                        readOnly: true,
                        minValue:0,
                        labelWidth: 90,
                        labelAlign: 'right',
                        allowBlank: false
                        
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
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                                expandExtForm = false;

                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T2Cleanup
            }
        ]
    });
    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            f.findField("MMNAME_C").setValue(args.MMNAME_C);
            f.findField("MMNAME_E").setValue(args.MMNAME_E);
            //f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
        }
    }

    function chkMMCODE(parMmcode) {
        var wh_no = T2Form.getForm().findField('WH_NO').getValue();
        Ext.Ajax.request({
            url: GetMmdataByMmcode,
            method: reqVal_p,
            params: { mmcode: parMmcode, wh_no: wh_no },
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success == false) {
                    Ext.Msg.alert('訊息', data.msg);
                    T2Form.getForm().findField('MMCODE').setValue('');
                    T2Form.getForm().findField('MMNAME_C').setValue('');
                    T2Form.getForm().findField('MMNAME_E').setValue('');
                    return;
                }

                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T2Form.getForm().findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                        T2Form.getForm().findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                    }
                    else {
                        Ext.Msg.alert('訊息', '院內碼不存在,請重新輸入!');
                        T2Form.getForm().findField('MMCODE').setValue('');
                        T2Form.getForm().findField('MMNAME_C').setValue('');
                        T2Form.getForm().findField('MMNAME_E').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function T2Submit() {
        var f = T2Form.getForm();

        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    var f2 = T2Form.getForm();
                    
                    switch (f2.findField("x").getValue()){
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            msglabel('訊息區:資料修改成功');
                    }

                    f2.findField("MMCODE").show();
                    f2.findField("MMCODE_DISPLAY").hide();

                    if (action.result.msg == 'Y') {
                        showExpWindow(T1LastRec.data.DOCNO, 'O', viewport);
                    }

                    T1Load();
                    T2Load();
                    myMask.hide();
                    Ext.getCmp('eastform').collapse();
                    T2Cleanup();
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

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        
        T2Form.getForm().findField('MMCODE').clearInvalid();
        T2Form.down('#cancel').hide();
        T2Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    //#region 2021-07-23 新增: 列印
    var reportUrl = '/Report/A/AA0035.aspx';
    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '調帳日期區間',
                labelAlign: 'right',
                name: 'T3P0',
                id: 'T3P0',
                vtype: 'dateRange',
                dateRange: { end: 'T3P1' },
                padding: '0 4 0 4',
                labelWidth: 80,
                width: 170,
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: 10,
                name: 'T3P1',
                id: 'T3P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T3P0' },
                padding: '0 4 0 4',
                width: 90,
                value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T3print', text: '列印', handler: function () {
                showReport();
            }
        },
        {
            itemId: 'T3cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '列印',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }
    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }
    function showReport() {

        var qstring = '?p0=' + T3Form.getForm().findField('T3P0').rawValue + '&p1=' + T3Form.getForm().findField('T3P1').rawValue;
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        hideWin3();
    }
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    //#endregion

    var window = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T3Grid],
        width: "570px",
        height: "300px",
        resizable: false,
        title:"細項",
        buttons: [{
            text: '關閉',
            handler: function () {
                this.up('window').hide();
            }
        }]
    });
    window.hide();


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
                    },
                    //{
                    //    itemId: 't3Grid',
                    //    region: 'south',
                    //    layout: 'fit',
                    //    collapsible: false,
                    //    title: '',
                    //    height: '34%',
                    //    split: true,
                    //    items: [T3Grid]
                    //}
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
                //items: [T1Query, T1Form]
                items: [T1Form, T2Form]
            }
        ]
    });
    
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);

    Ext.getCmp('btnUpdate3').setDisabled(true);
    Ext.getCmp('btnCancel3').setDisabled(true);
});