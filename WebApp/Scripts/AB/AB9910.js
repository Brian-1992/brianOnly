Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '手動申請';
    var userId, userName, userInid, userInidName;

    var viewModel = Ext.create('WEBAPP.store.AB.AB0010VM');
    var UserInfoStore = viewModel.getStore('USER_INFO');
    function UserInfoLoad() {
        UserInfoStore.load(function (records, operation, success) {
            if (success) {
                var r = records[0];
                userId = r.get('TUSER');
                userName = r.get('UNA');
                userInid = r.get('INID');
                userInidName = r.get('INID_NAME');
            }
        });
    }

    var T1Store = viewModel.getStore('ME_DOCM');
    T1Store.on('load', function () {
        var recNew = Ext.create('WEBAPP.model.ME_DOCM');
        recNew.data.DOCNO = '*';
        T1Store.add(recNew);
    });
    function T1Load() {
        //T1Store.getProxy().setExtraParam("field1", T1Query.getForm().findField('field1').getValue());
        //T1Store.getProxy().setExtraParam("field2", T1Query.getForm().findField('field2').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", userId);
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        //T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("DOCTYPE", 'MR');
        T1Store.getProxy().setExtraParam("FLOWID", '0101'); 
        T1Tool.moveFirst();

        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                },
                callback: function () {
                        var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                        recNew.data.DOCNO = '*';
                        T2Store.add(recNew);
                }
            });
        }
    }

    var T3Store = viewModel.getStore('SUPPLY_INID');
    function T3Load() {
        //T3Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T3Store.on('load', function () {
            var f = T1Form.getForm();
            f.findField("TOWH").setValue(T3Store.getAt(0).get('WH_NO'));

            Ext.getCmp('FRWH').setValue('');
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: T3Store.getAt(0).get('WH_NO')
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    T4Load(data);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
                //jsonData: data
            });
        });
    }

    var T4Store = viewModel.getStore('FRWH');
    function T4Load(wh_grade) {
        T4Store.getProxy().setExtraParam("WH_GRADE", wh_grade);
        T4Store.on('load', function () {
            var f = T1Form.getForm();
            f.findField("FRWH").setValue(T4Store.getAt(0).get('WH_NO'));
        });

       T4Store.load({
            params: {
                start: 0
            }
        });
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
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
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '單據號碼',
                        name: 'P1',
                        id: 'P1',
                        enforceMaxLength: true,
                        maxLength: 21,
                        labelWidth: 60,
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '申請庫房代碼',
                        name: 'P4',
                        id: 'P4',
                        enforceMaxLength: true,
                        maxLength: 6,
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '核撥庫房代碼',
                        name: 'P6',
                        id: 'P6',
                        enforceMaxLength: true,
                        maxLength: 8,
                        width: 170,
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T2Store.removeAll();
                            T1Grid.getSelectionModel().deselectAll();
                            T1Load();
                            msglabel('訊息區:');
                            Ext.getCmp('btnUpdate').setDisabled(true);
                            Ext.getCmp('btnDel').setDisabled(true);
                            Ext.getCmp('btnSubmit').setDisabled(true);
                            Ext.getCmp('btnAdd2').setDisabled(true);
                            Ext.getCmp('btnUpdate2').setDisabled(true);
                            Ext.getCmp('btnDel2').setDisabled(true);
                            Ext.getCmp('btnCopy').setDisabled(true);
                            //Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
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
                hidden: true,
                handler: function () {
                    //var r = WEBAPP.model.ME_DOCM.create({
                    //    //APPNAME: 'aaa'
                    //});
                    //T1Store.add(r.copy());
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0010/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                hidden: true,
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0010/MasterUpdate';
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
                        selection.map(item => {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        });

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T2Store.removeAll();
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
            {
                text: '複製',
                id: 'btnCopy',
                name: 'btnCopy',
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

                        Ext.MessageBox.confirm('訊息', '確認是否複製?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/Copy',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('複製成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnCopy').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '提出申請',
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

                        Ext.MessageBox.confirm('訊息', '確認是否提出申請?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/UpdateStatus',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
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
                layout: 'fit',
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
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 100
            },
            {
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 150
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 130
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "申請庫房",
                dataIndex: 'TOWH_NAME',
                width: 130,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
            },
            {
                text: "核撥庫房",
                dataIndex: 'FRWH_NAME',
                width: 140,
                //editor: {
                //    xtype: 'combo',
                //    store: T3Store,
                //    displayField: 'WH_NAME',
                //    valueField: 'WH_NO'
                //}
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
                        T2Form.setVisible(false);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(false);
                        Ext.getCmp('btnSubmit').setDisabled(false);

                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);
                        Ext.getCmp('btnAdd2').setDisabled(false);
                        Ext.getCmp('btnCopy').setDisabled(false);

                        if (T1Set === '')
                            setFormT1a();
                        T2Load();

                        if (T1LastRec.data.DOCNO == '*')
                            T1Tool.down('#btnAdd').click();
                        else
                            T1Tool.down('#btnUpdate').click();
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                T2Load();

                if (T1LastRec == null)
                    Ext.getCmp('btnAdd2').setDisabled(true);
            }
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
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
                hidden: true,
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0010/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                hidden: true,
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0010/DetailUpdate';
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
                        //let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
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
            },
            {
                text: "核撥日期",
                dataIndex: 'APVTIME',
                width: 70,
            },
            {
                text: "點收數量",
                dataIndex: 'ACKQTY',
                width: 70,
            },
            {
                text: "點收日期",
                dataIndex: 'ACKTIME',
                width: 70,
            },
            {
                text: "建議申請量",
                dataIndex: 'SUGGEST_QTY',
                width: 100,
                renderer: function (val, meta, record) {
                    if (val != null)
                        //return '<a href="javascript:popWinForm(' + val + ",'" + record.data['TMPKEY'] + "')\" >" + val + '</a>';
                        //return '<a href="javascript:popWinForm(' + val + ')" >' + val + '</a>';
                        return "<a href=\"javascript:popWinForm('" + record.data['WH_NO'] + "','" + record.data['MMCODE'] + "')\" >" + val + '</a>';
                }
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
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {
                        Ext.getCmp('btnUpdate').setDisabled(true);
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnSubmit').setDisabled(true);

                        Ext.getCmp('btnUpdate2').setDisabled(false);
                        Ext.getCmp('btnDel2').setDisabled(false);
                        Ext.getCmp('btnAdd2').setDisabled(false);
                        Ext.getCmp('btnCopy').setDisabled(true);

                        if (T1Set === '')
                            setFormT2a();

                        
                        if (T2LastRec.data.DOCNO == '*')
                            T2Tool.down('#btnAdd2').click();
                        else
                            T2Tool.down('#btnUpdate2').click();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

    popWinForm = function (wh_no, mmcode) {
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: {
                    type: 'table',
                    columns: 2
                },
                closable: false,
                //html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                //items: [T3Form],
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '安全日',
                        name: 'SAFE_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '安全量',
                        name: 'SAFE_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '作業日',
                        name: 'OPER_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '作業量',
                        name: 'OPER_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '運補日',
                        name: 'SHIP_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '運補量',
                        name: 'SHIP_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '基準量',
                        name: 'HIGH_QTY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '現有庫存量',
                        name: 'INV_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最低庫存量',
                        name: 'LOW_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '建議申請量',
                        name: 'SUGGEST_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '日平均消耗量',
                        name: 'DAVG_USEQTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小撥補量',
                        name: 'MIN_ORDQTY',
                        labelAlign: 'right',
                    },
                ],
                //buttons: [{ xtype: 'label', id: 'DivResult', text: '上傳結果', width: '50%', color: 'blue' },
                buttons: [
                    {
                        id: 'winclosed',
                        disabled: false,
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }
                ]
            });
            //var win = GetPopWin(viewport, popform, '製造商證照（公司登記/工廠登記/營業執照….）', viewport.width - 20, viewport.height - 20);
            var win = GetPopWin(viewport, popform, '明細', 400, 300);

            var QtyStore = viewModel.getStore('SUGGEST_QTY');
            QtyStore.getProxy().setExtraParam("WH_NO", wh_no);
            QtyStore.getProxy().setExtraParam("MMCODE", mmcode);
            QtyStore.load(function () {
                var f = popform.getForm();
                f.findField("SAFE_DAY").setValue(this.getAt(0).get('SAFE_DAY'));
                f.findField("SAFE_QTY").setValue(this.getAt(0).get('SAFE_QTY'));
                f.findField("OPER_DAY").setValue(this.getAt(0).get('OPER_DAY'));
                f.findField("OPER_QTY").setValue(this.getAt(0).get('OPER_QTY'));
                f.findField("SHIP_DAY").setValue(this.getAt(0).get('SHIP_DAY'));
                f.findField("SHIP_QTY").setValue(this.getAt(0).get('SHIP_QTY'));
                f.findField("HIGH_QTY").setValue(this.getAt(0).get('HIGH_QTY'));
                f.findField("INV_QTY").setValue(this.getAt(0).get('INV_QTY'));
                f.findField("LOW_QTY").setValue(this.getAt(0).get('LOW_QTY'));
                f.findField("SUGGEST_QTY").setValue(this.getAt(0).get('SUGGEST_QTY'));
                f.findField("DAVG_USEQTY").setValue(this.getAt(0).get('DAVG_USEQTY'));
                f.findField("MIN_ORDQTY").setValue(this.getAt(0).get('MIN_ORDQTY'));
            });
        }
        win.show();
    }

    Date.prototype.yyymmdd = function () {
        var mm = this.getMonth() + 1; // getMonth() is zero-based
        var dd = this.getDate();

        return [this.getFullYear() - 1911,
        (mm > 9 ? '' : '0') + mm,
        (dd > 9 ? '' : '0') + dd
        ].join('');
    };

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        //viewport.down('#t1Grid').mask();
        //viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/MiUnitexch.js
            //T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("TOWH"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A

            var date = new Date();

            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(date.yyymmdd());
        }
        else {
            u = f.findField('TOWH');

            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            f.findField("TOWH").setValue(tmpArray[0]);

            // 因為使用部門和領用庫房有連動, 所以都要重新取得
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: tmpArray[0]
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    T4Load(data);

                    tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
                    f.findField('TOWH').setValue(tmpArray[0]);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
            });
            
        }
        f.findField('x').setValue(x);
        f.findField('TOWH').setReadOnly(false);
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
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            f.findField('TOWH').setValue(tmpArray[0]);

            // 因為使用部門和領用庫房有連動, 所以都要重新取得
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: tmpArray[0]
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    T4Load(data);

                    tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");

                    // 切換T1Grid時, 如果值一樣,還做setValue()的話,欄位會被清空,所以值不同再做setValue()
                    if (f.findField('FRWH').getValue() != tmpArray[0])
                        f.findField('FRWH').setValue(tmpArray[0]);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
            });

            f.findField('TOWH').setReadOnly(true);
            f.findField('FRWH').setReadOnly(true);
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
                fieldLabel: '申請單號',
                name: 'DOCNO',
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
                fieldLabel: '申請部門',
                name: 'INID_NAME',
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
                fieldLabel: '申請庫房',
                name: 'TOWH',
                store: T3Store,
                displayField: 'WH_NAME',
                valueField: 'WH_NO',
                readOnly: true,
                allowBlank: false, // 欄位為必填
                //anyMatch: true,
                //typeAhead: true,
                //forceSelection: true,
                //queryMode: 'local',
                //triggerAction: 'all',
                fieldCls: 'required',
                listeners: {
                    //change: function (e, newValue, oldValue, eOpts) {
                    select: function (combo, record, eOpts) {
                        Ext.getCmp('FRWH').setValue('');
                        Ext.Ajax.request({
                            url: '/api/AB0010/GetGrade',
                            method: reqVal_p,
                            params: {
                                //WH_NO: newValue
                                WH_NO: record.get("WH_NO")
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                T4Load(data);
                                //Ext.getCmp('TOWH').bindStore(T4Store);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                name: 'FRWH',
                id: 'FRWH',
                store: T4Store,
                displayField: 'WH_NAME',
                valueField: 'WH_NO',
                readOnly: true,
                allowBlank: false, // 欄位為必填
                //anyMatch: true,
                //typeAhead: true,
                //forceSelection: true,
                //queryMode: 'local',
                //triggerAction: 'all',
                fieldCls: 'required'
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
                                //T1Set = '';
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
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T2Store.removeAll();
                            T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
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
        viewport.down('#t2Grid').unmask();
        //Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
    }

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        //viewport.down('#t1Grid').mask();
        //viewport.down('#t2Grid').mask();
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
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
        }
        f.findField('x').setValue(x);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        T2Form.down('#btnMmcode').setVisible(true);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            T2Form.down('#btnMmcode').setVisible(false);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            f.findField("MMNAME_C").setValue(args.MMNAME_C);
            f.findField("MMNAME_E").setValue(args.MMNAME_E);
            f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
        }
    }

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        labelAlign: 'right',
        msgTarget: 'side',
        labelWidth: 60,
        width: 300,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0010/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

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
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                }
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60,
            width: 300,
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
                fieldLabel: '核撥庫房',
                name: 'FRWH2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    editable: false,
            //    submitValue: true,
            //    allowBlank: false,
            //    fieldCls: 'required',
            //    listeners: {
            //        render: function () {
            //            this.getEl().on('mousedown', function (e, t, eOpts) {
            //                var f = T2Form.getForm();
            //                if (!f.findField("MMCODE").readOnly) {
            //                    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //                    popMmcodeForm(viewport, { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0], closeCallback: setMmcode });
            //                }
            //            });
            //        }
            //    }
            //},
            {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 7 7 0',
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
                xtype: 'textfield',
                fieldLabel: '申請數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true,
                allowBlank: false,
                fieldCls: 'required',
            },
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
                                T2Submit();
                                //T1Set = '';
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

    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.DN);
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                    }

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
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        //Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

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
                width: '70%',
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '30%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: false,
                floatable: false,
                width: '30%',
                title: '瀏覽',
                border: false,
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
    UserInfoLoad();
    T3Load();
    //Ext.getCmp('eastform').collapse();

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnCopy').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
});