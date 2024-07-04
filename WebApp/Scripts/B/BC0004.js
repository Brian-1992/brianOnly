/*修改記錄 1090220
1.衛保室要求[CHARGE]與MI_MAST[M_PAYKIND]連動
2.[PRICE],[CHARGE]...有院內碼時,不能修改
3.考量有很多[PRICE]=0資料,所以開放[消審會]可以修改
*/
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.FileGridField', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {
    var T1Set = '';
    var T11Set = '';
    var GetInidByTuser = '/api/BC0002/GetInidByTuser';
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var GetMmdataByMmcode = '/api/BC0002/GetMmdataByMmcode';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var AgennoComboGet = '/api/BC0002/GetAgennoCombo';
    var T1GetExcel = '../../../api/BC0004/Excel';
    var MmcodeComboGet = '/api/BC0002/GetMmcodeCombo';
    var reportUrl = '/Report/B/BC0002.aspx';
    var T1Name = "小額採購消審會審核";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var T12Rec = 0;
    var T12LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //庫房別ComboStore
    var st_Agenno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BC0004/GetAgennoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var cb = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'P3',
        id: 'P3',
        fieldLabel: '狀態碼',
        queryParam: {
            GRP_CODE: 'PH_SMALL_M',
            DATA_NAME: 'STATUS'
        },
        width: 260,
        labelWidth: 60,
        insertEmptyRow: true,
        autoSelect: true,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            bodyStyle: 'padding: 3px 0px;',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '申請單號',
                    name: 'P0',
                    id: 'P0',
                    width: 210,
                    enforceMaxLength: true,
                    maxLength: 22
                }, {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '申請日期',
                    width: 170,
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
                    labelSeparator: '',
                    width: 130,
                    labelWidth: 20,
                    listeners: {
                        change: function (field, nVal, oVal, eOpts) {
                            T1Query.getForm().findField('P1').setMaxValue(nVal);
                        }
                    }
                }, cb,
                {
                    xtype: 'button',
                    id: 'QryButton',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            bodyStyle: 'padding: 3px 0px;',
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    store: st_Agenno,
                    name: 'P4',
                    id: 'P4',
                    width: 220,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                },
                {
                    xtype: 'button',
                    text: '產生訂單',
                    itemId: 'genPo',
                    disabled: true,
                    handler: function () {
                        if (T1Query.getForm().findField('P4').getValue() == "" || T1Query.getForm().findField('P4').getValue() == null) {
                            Ext.Msg.alert('訊息', '[庫房別]欄不可空白!');
                        }
                        else {
                            Ext.Ajax.request({
                                url: '/api/BC0004/CheckAndCreatOrder',
                                method: reqVal_p,
                                params: {
                                    p0: T1LastRec.data['DN'],
                                    p4: T1Query.getForm().findField('P4').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        if (data.msg != "") {
                                            Ext.MessageBox.alert('訊息', data.msg);
                                        }
                                        else {
                                            Ext.Msg.alert('訊息', '產生訂單成功');
                                            //T1Load();
                                            var f1 = T1Form.getForm();
                                            var r = f1.getRecord();
                                            var v = data.etts[0];
                                            r.set(v);
                                            r.commit();
                                            T1Grid.getView().refresh();
                                            msglabel('產生訂單成功');
                                            T1Query.down('#genPo').setDisabled(true);
                                        }
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.PhSmallM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: session['UserId'],
                    p5: 'Audit2'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }
    var T11Store = Ext.create('WEBAPP.store.PhSmallD', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Form.getForm().findField('DN').getValue(),
                    p1: '0'

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T11Load() {
        T11Tool.moveFirst();
    }
    var T12Store = Ext.create('WEBAPP.store.PhSmallD', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Form.getForm().findField('DN').getValue(),
                    p1: '1'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T12Load() {
        T12Tool.moveFirst();
    }
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1edit', text: '修改', disabled: true, id: 't1edit', handler: function () {
                    T1Set = '/api/BC0004/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1reject', text: '剔退', disabled: true, id: 't1reject',
                handler: function () {
                    msglabel('訊息區:');
                    // T1Form.getForm().findField('DEPT').setValue('衛材補給保養室');
                    popRejectReason();
                }
            }, {
                itemId: 't1approve', text: '核可', disabled: true, id:'t1approve', handler: function () {
                    msglabel('訊息區:');
                    var rtnChk = chkFormEmpty();
                    if (rtnChk == true) {
                        Ext.MessageBox.confirm('核可', '確定要【核可】？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/BC0004/MasterApprove';
                                //T1Form.getForm().findField('DEPT').setValue('衛材補給保養室');
                                T1Form.getForm().findField('x').setValue('A');
                                T1Submit();
                            }
                        }
                        );
                    }
                    else
                        Ext.Msg.alert('訊息', rtnChk);
                }
            }, {
            itemId: 't1print', text: '列印', disabled: true, id: 't1print', handler: function () {
                    if (T11Store.getCount() > 0)
                        showReport();
                    else
                        Ext.Msg.alert('訊息', '請先建立明細資料');
                }
            }, {
                itemId: 't1export', text: '匯出', disabled: true, id: 't1export',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: T1LastRec.data['DN'] + '明細檔.xls' });
                    p.push({ name: 'p0', value: T1LastRec.data['DN'] });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        T1Form.setVisible(true);
        T11Form.setVisible(false);
        T12Form.setVisible(false);

        if (x === "I") {
            //isNew = true;
            //var r = Ext.create('WEBAPP.model.PhSmallM');
            //T1Form.loadRecord(r);
            //f.clearInvalid();
            //f.findField('DN').setValue('系統自編');
            //f.findField('APPMAN').setValue(parent.parent.userName);
            //getINID(parent.parent.userId);
            //u = f.findField('ACCEPT');
        }
        else {
            if (f.findField('DELIVERY').getValue() == '') f.findField('DELIVERY').setValue('中央庫房');
            if (f.findField('ACCEPT').getValue() == '') f.findField('ACCEPT').setValue('目視驗收');
            if (f.findField('PAYWAY').getValue() == '') f.findField('PAYWAY').setValue('匯款');
            u = f.findField('USEWHERE');
        }
        // f.findField('DEPT').setValue('衛材補給保養室');
        f.findField('APP_USER1').setValue(session['UserName']);
        f.findField('x').setValue(x);
        f.findField('ACCEPT').setReadOnly(false);
        //f.findField('AGEN_NO').setReadOnly(false);
        f.findField('ALT').setReadOnly(false);
        f.findField('DELIVERY').setReadOnly(false);
        f.findField('DEMAND').setReadOnly(false);
        f.findField('DUEDATE').setReadOnly(false);
        f.findField('OTHERS').setReadOnly(false);
        f.findField('PAYWAY').setReadOnly(false);
        f.findField('TEL').setReadOnly(false);
        f.findField('USEWHEN').setReadOnly(false);
        f.findField('USEWHERE').setReadOnly(false);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        u.focus();
    }

    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't11add', id:'t11add', text: '新增', disabled: true, handler: function () {
                    T11Set = '/api/BC0002/DetailCreate';
                    setFormT11('I', '新增');
                }
            },
            {
                itemId: 't11edit', id:'t11edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '/api/BC0002/DetailUpdate';
                    setFormT11("U", '修改');
                }
            }, {
                itemId: 't11delete', id:'t11delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Set = '/api/BC0002/DetailDelete';
                            T11Form.getForm().findField('x').setValue('D');
                            T11Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
        displayInfo: true,
        border: false,
        plain: true,
        //buttons: [
        //    {
        //        itemId: 't11add', text: '新增', disabled: true, handler: function () {
        //            T11Set = '/api/BC0002/DetailCreate';
        //            setFormT11('I', '新增');
        //        }
        //    },
        //    {
        //        itemId: 't11edit', text: '修改', disabled: true, handler: function () {
        //            T11Set = '/api/BC0002/DetailUpdate';
        //            setFormT11("U", '修改');
        //        }
        //    }, {
        //        itemId: 't11delete', text: '刪除', disabled: true,
        //        handler: function () {
        //            Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
        //                if (btn === 'yes') {
        //                    T11Set = '/api/BC0002/DetailDelete';
        //                    T11Form.getForm().findField('x').setValue('D');
        //                    T11Submit();
        //                }
        //            }
        //            );
        //        }
        //    }
        //]
    });
    function setFormT11(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T11Form.getForm();
        T1Form.setVisible(false);
        T11Form.setVisible(true);
        T12Form.setVisible(false);

        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhSmallD');
            T11Form.loadRecord(r);
            f.clearInvalid();
            f.findField('SEQ').setValue('系統自編');
            f.findField('DN').setValue(T1LastRec.data['DN']);
            f.findField('UK').setReadOnly(false);
            f.findField('INIDNAME').setValue('');
            u = f.findField("NMSPEC");
        }
        else {
            u = f.findField('NMSPEC');
        }
        if (f.findField('MMCODE').getValue() == null || f.findField('MMCODE').getValue() == "") {
            f.findField('CHARGE').setReadOnly(false);
            f.findField('CHARGE').setFieldStyle("background: pink;");
            f.findField('NMSPEC').setReadOnly(false);
            f.findField('NMSPEC').setFieldStyle("background: pink;");
            f.findField('UNIT').setReadOnly(false);
            f.findField('UNIT').setFieldStyle("background: pink;");
            f.findField('PRICE').setReadOnly(false);
            f.findField('PRICE').setFieldStyle("background: pink;");
        }
        else {
            f.findField('CHARGE').setReadOnly(true);
            f.findField('CHARGE').setFieldStyle("background: #CCCCCC"); //淺灰
            f.findField('NMSPEC').setReadOnly(true);
            f.findField('NMSPEC').setFieldStyle("background: #CCCCCC");
            f.findField('UNIT').setReadOnly(true);
            f.findField('UNIT').setFieldStyle("background: #CCCCCC");
            //f.findField('PRICE').setReadOnly(true);  
            //f.findField('PRICE').setFieldStyle("background: #CCCCCC");
            //f.findField('PRICE').clearInvalid();
        }
        f.findField('x').setValue(x);
        f.findField('MEMO').setReadOnly(false);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('QTY').setReadOnly(false);
        f.findField('INID').setReadOnly(false);

        T11Form.down('#t11cancel').setVisible(true);
        T11Form.down('#t11submit').setVisible(true);
        u.focus();
    }
    function setFormT12(x, t) {
        //viewport.down('#t1Grid').mask();
        //viewport.down('#t11Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        //viewport.down('#form').expand();
        //var f = T11Form.getForm();
        //T1Form.setVisible(false);
        //T11Form.setVisible(true);
        //if (x === "I") {
        //    isNew = true;
        //    var r = Ext.create('WEBAPP.model.PhSmallD');
        //    T11Form.loadRecord(r);
        //    f.clearInvalid();
        //    f.findField('SEQ').setValue('系統自編');
        //    f.findField('DN').setValue(T1LastRec.data['DN']);
        //    u = f.findField("NMSPEC");
        //}
        //else {
        //    u = f.findField('NMSPEC');
        //}
        //f.findField('x').setValue(x);
        //f.findField('MEMO').setReadOnly(false);
        //f.findField('UK').setReadOnly(false);
        //f.findField('MMCODE').setReadOnly(false);
        //f.findField('NMSPEC').setReadOnly(false);
        //f.findField('UNIT').setReadOnly(false);
        //f.findField('PRICE').setReadOnly(false);
        //f.findField('QTY').setReadOnly(false);
        //T11Form.down('#t11cancel').setVisible(true);
        //T11Form.down('#t11submit').setVisible(true);
        //u.focus();
    }
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
            text: "申請單號",
            dataIndex: 'DN',
            width: 150
        }, {
            text: "用途",
            dataIndex: 'USEWHERE',
            width: 140
        }, {
            text: "其他",
            dataIndex: 'OTHERS',
            width: 140
        }, {
            text: "申請人責任中心",
            dataIndex: 'APP_INID',
            width: 130
        }, {
            text: "申請人姓名",
            dataIndex: 'APP_USER',
            width: 80
        }, {
            text: "狀態碼",
            dataIndex: 'STATUS',
            width: 120
        }, {
            xtype: 'datecolumn',
            text: "申請日期",
            dataIndex: 'APPTIME',
            format: 'Xmd',
            width: 80
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 150
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
                        T11Form.setVisible(false);
                        T12Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    if (T1LastRec.data.IS_CR == 'Y') {
                        
                        Ext.getCmp('t11add').disable();
                        Ext.getCmp('t11edit').disable();
                        Ext.getCmp('t11delete').disable();
                        Ext.getCmp('t1reject').disable();
                        Ext.getCmp('t1edit').disable();
                    }
                    
                }

                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
                T11Load();
                T12Load();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            if (T1LastRec.data['STATUS'].split(' ')[0] == 'E')
                T1Query.down('#genPo').setDisabled(false);
            else
                T1Query.down('#genPo').setDisabled(true);
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'].split(' ')[0] == 'C') {
                T1Grid.down('#t1edit').setDisabled(false);
                T1Grid.down('#t1reject').setDisabled(false);
                T1Grid.down('#t1approve').setDisabled(false);
                T11Grid.down('#t11add').setDisabled(false);
            }
            else {
                T1Grid.down('#t1edit').setDisabled(true);
                T1Grid.down('#t1reject').setDisabled(true);
                T1Grid.down('#t1approve').setDisabled(true);
                T11Grid.down('#t11add').setDisabled(true);
            }
            T1Grid.down('#t1export').setDisabled(false);
            T1Grid.down('#t1print').setDisabled(false);

            if (T1LastRec.data.IS_CR == 'Y') {
                
                Ext.getCmp('t11add').disable();
                Ext.getCmp('t11edit').disable();
                Ext.getCmp('t11delete').disable();
                Ext.getCmp('t1reject').disable();
                Ext.getCmp('t1edit').disable();
            }
        }
        else {
            T1Query.down('#genPo').setDisabled(true);
            T1Form.getForm().reset();
            T1Grid.down('#t1reject').setDisabled(true);
            T1Grid.down('#t1approve').setDisabled(true);
            T1Grid.down('#t1export').setDisabled(true);
            T1Grid.down('#t1print').setDisabled(true);
            T11Grid.down('#t11add').setDisabled(true);
            T11Grid.down('#t11edit').setDisabled(true);
            T11Grid.down('#t11delete').setDisabled(true);
        }

    }

    var T11Grid = Ext.create('Ext.grid.Panel', {
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "項次",
            dataIndex: 'SEQ',
            style: 'text-align:left',
            align: 'right',
            width: 40
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "",
            dataIndex: 'OLD_MMCODE',
            hidden: true
        }, {
            text: "需求單位",
            dataIndex: 'INID',
            width: 150
        }, {
            text: "",
            dataIndex: 'OLD_INID',
            hidden: true
        }, {
            text: "衛材計價方式",
            dataIndex: 'CHARGE',
            width: 140
        }, {
            text: "品名及規格廠牌",
            dataIndex: 'NMSPEC',
            width: 150
        }, {
            text: "品名及規格廠牌",
            dataIndex: 'NMSPEC',
            width: 150
        }, {
            text: "數量",
            dataIndex: 'QTY',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "計量單位",
            dataIndex: 'UNIT',
            width: 70
        }, {
            text: "單價",
            dataIndex: 'PRICE',
            style: 'text-align:left',
            align: 'right',
            width: 60
        }, {
            text: "總價",
            dataIndex: 'TOTAL_PRICE',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "備考",
            dataIndex: 'MEMO',
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T11Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(true);
                        T12Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];

                if (T1LastRec.data.IS_CR == 'Y') {
                    Ext.getCmp('t11add').disable();
                    Ext.getCmp('t11edit').disable();
                    Ext.getCmp('t11delete').disable();
                    Ext.getCmp('reject').disable();
                }

                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT11a();
            }
        }
    });
    function setFormT11a() {
        T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T11LastRec) {
            isNew = false;
            T11Form.loadRecord(T11LastRec);
            T11Form.getForm().findField('CHARGE').setValue(T11LastRec.data['CHARGE'].split(' ')[0]);
            T11Form.getForm().findField('INID').setValue(T11LastRec.data['INID'].split(' ')[0]);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'].split(' ')[0] != 'C') {
                // 主檔非待消審會審核的資料就不允許異動
                T11Grid.down('#t11add').setDisabled(true);
                T11Grid.down('#t11edit').setDisabled(true);
                T11Grid.down('#t11delete').setDisabled(true);
            }
            f.findField('CHARGE').setReadOnly(false);
            f.findField('CHARGE').setFieldStyle("background: pink;");
            f.findField('NMSPEC').setReadOnly(false);
            f.findField('NMSPEC').setFieldStyle("background: pink;");
            f.findField('UNIT').setReadOnly(false);
            f.findField('UNIT').setFieldStyle("background: pink;");
            f.findField('PRICE').setReadOnly(false);
            f.findField('PRICE').setFieldStyle("background: pink;");
        }
        else {
            T11Form.getForm().reset();
        }
    }

    var T12Grid = Ext.create('Ext.grid.Panel', {
        store: T12Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T12Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "品名及規格廠牌",
            dataIndex: 'NMSPEC',
            width: 150
        }, {
            text: "單品需求總量",
            dataIndex: 'SUMQTY',
            width: 100,
            style: 'text-align:left',
            align: 'right'
        }, {
            text: "計量單位",
            dataIndex: 'UNIT',
            width: 70,
            style: 'text-align:left',
            align: 'left'
        }, {
            text: "單價",
            dataIndex: 'PRICE',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "單品需求總價",
            dataIndex: 'SUMPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 140
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T12Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(false);
                        T12Form.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                T12Rec = records.length;
                T12LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT12a();
            }
        }
    });

    function setFormT12a() {
        //T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        //T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T12LastRec) {
            isNew = false;
            T12Form.loadRecord(T12LastRec);
            //T11Form.getForm().findField('CHARGE').setValue(T11LastRec.data['CHARGE'].split(' ')[0]);
            //T11Form.getForm().findField('INID').setValue(T11LastRec.data['INID'].split(' ')[0]);
            var f = T12Form.getForm();
            f.findField('x').setValue('U');
            //if (T1LastRec.data['STATUS'].split(' ')[0] != 'C') {
            //    // 主檔非待消審會審核的資料就不允許異動
            //    T11Grid.down('#t11add').setDisabled(true);
            //    T11Grid.down('#t11edit').setDisabled(true);
            //    T11Grid.down('#t11delete').setDisabled(true);
            //}
        }
        else {
            T12Form.getForm().reset();
        }
    }

    //var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
    //    name: 'AGEN_NO',
    //    fieldLabel: '廠商代碼',
    //    limit: 20,
    //    queryUrl: AgennoComboGet,
    //    //storeAutoLoad: true,
    //    insertEmptyRow: true,
    //    readOnly: true,
    //    listeners: {
    //        blur: function (field, eve, eOpts) {
    //            var chkType = T1Form.getForm().findField('x').getValue();
    //            if (field.getValue() != '' && field.getValue() != null
    //                && field.readOnly == false)
    //                chkAGENNO(field.getValue());
    //            else
    //                T1Form.getForm().findField('AGEN_NAMEC').setValue('');
    //        }
    //    }
    //});
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
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DN',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'textarea',
            fieldLabel: '用途',
            name: 'USEWHERE',
            enforceMaxLength: true,
            maxLength: 100,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '需求',
            name: 'DEMAND',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '替代',
            name: 'ALT',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '本次申請量<br>預估使用時間',
            name: 'USEWHEN',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '電話',
            name: 'TEL',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請人姓名',
            name: 'APP_USER',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            submitValue: false
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請人責任中心',
            name: 'APP_INID',
            enforceMaxLength: true,
            maxLength: 8,
            readOnly: true,
            submitValue: false
        }, {
            xtype: 'textarea',
            fieldLabel: '其他',
            name: 'OTHERS',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            fieldLabel: '交貨期限',
            name: 'DUEDATE',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: '交貨地點',
            name: 'DELIVERY',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: '驗收方式',
            name: 'ACCEPT',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: '付款方式',
            name: 'PAYWAY',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
            //    T1FormAgenno, {
            //    xtype: 'displayfield',
            //    fieldLabel: '廠商名稱',
            //    name: 'AGEN_NAMEC',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    readOnly: true,
            //    submitValue: true
            //}, 
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單位審核主管',
            name: 'APP_USER1',
            readOnly: true,
            submitValue: true,
            hidden: true
        }, {
            xtype: 'textarea',
            fieldLabel: '簽審歷程',
            name: 'SIGNDATA',
            enforceMaxLength: true,
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '剔退說明',
            name: 'REASON',
            readOnly: true,
            submitValue: true,
            renderer: function (val, meta, record) {
                return '<font color="red">' + val + '</font>';
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '承辦人',
            name: 'DO_USER',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '承辦單位',
            name: 'DEPT',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態碼',
            name: 'STATUS',
            readOnly: true,
            submitValue: true
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                            msglabel('資料修改成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T11Grid.down('#t11add').setDisabled(true);
                            T11Grid.down('#t11edit').setDisabled(true);
                            T11Grid.down('#t11delete').setDisabled(true);
                            if (action.result.msg == '')
                                msglabel('核可完成');
                            else
                                Ext.Msg.alert('訊息', action.result.msg);
                            break;
                        case "R":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            callableWin.destroy();
                            callableWin = null;
                            msglabel('剔退完成');
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
        viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }

    var T11FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        limit: 20,
        queryUrl: MmcodeComboGet,
        //storeAutoLoad: true,
        insertEmptyRow: true,
        readOnly: true,
        listeners: {
            select: function (field, record, eOpts) {
                if (field.getValue() != '' && field.getValue() != null && field.readOnly == false)
                    chkMMCODE(field.getValue());
            },
            blur() {
                var f = T11Form.getForm();
                if (f.findField('MMCODE').getValue() != '' && f.findField('MMCODE').getValue() != null && f.findField('MMCODE').readOnly == false)
                    chkMMCODE(f.findField('MMCODE').getValue());
                //if (f.findField('MMCODE').getValue() == null || f.findField('MMCODE').getValue() == "") {
                //    f.findField('CHARGE').setReadOnly(false);
                //    f.findField('CHARGE').setFieldStyle("background: pink;");
                //    f.findField('NMSPEC').setReadOnly(false);
                //    f.findField('NMSPEC').setFieldStyle("background: pink;");
                //    f.findField('UNIT').setReadOnly(false);
                //    f.findField('UNIT').setFieldStyle("background: pink;");
                //    f.findField('PRICE').setReadOnly(false);
                //    f.findField('PRICE').setFieldStyle("background: pink;");
                //    f.findField('CHARGE').setValue('');
                //    f.findField('NMSPEC').setValue('');
                //    f.findField('UNIT').setValue('');
                //    f.findField('PRICE').setValue('');
                //    f.findField('QTY').setValue('');
                //}
                //else {
                //    f.findField('CHARGE').setReadOnly(true);
                //    f.findField('CHARGE').setFieldStyle("background: #CCCCCC"); //淺灰
                //    f.findField('NMSPEC').setReadOnly(true);
                //    f.findField('NMSPEC').setFieldStyle("background: #CCCCCC");
                //    f.findField('UNIT').setReadOnly(true);
                //    f.findField('UNIT').setFieldStyle("background: #CCCCCC");
                //    //f.findField('PRICE').setReadOnly(true);
                //    //f.findField('PRICE').setFieldStyle("background: #CCCCCC");
                //    //f.findField('PRICE').clearInvalid();
                //}
            }
        }
    });
    var T11FormCharge = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'CHARGE',
        fieldLabel: '衛材計價方式',
        queryParam: {
            GRP_CODE: 'PH_SMALL_M',
            DATA_NAME: 'CHARGE'
        },
        readOnly: true,
        autoSelect: true,
        allowBlank: false,
        fieldCls: 'required',
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var T11FormInid = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '需求單位代碼',
        limit: 20,
        queryUrl: InidComboGet,
        //storeAutoLoad: true,
        insertEmptyRow: true,
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        listeners: {
            select: function (c, r, i, e) {
                T11Form.getForm().findField('INIDNAME').setValue(r.get('INID_NAME'));
            }
        }
    });
    var T11Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
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
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: 'OLD_MMCODE',
            name: 'OLD_MMCODE',
            xtype: 'hidden'
        }, {
            fieldLabel: 'OLD_NMSPEC',
            name: 'OLD_NMSPEC',
            xtype: 'hidden'
        }, {
            fieldLabel: 'OLD_INID',
            name: 'OLD_INID',
            xtype: 'hidden'
        }, {
            fieldLabel: '申請單號',
            name: 'DN',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '項次',
            name: 'SEQ',
            readOnly: true,
            submitValue: true
        }, T11FormMmcode, T11FormInid, {
            xtype: 'displayfield',
            fieldLabel: '申購單位',
            name: 'INIDNAME',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            submitValue: true
        }, T11FormCharge, {
            fieldLabel: '品名及規格廠牌',
            name: 'NMSPEC',
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '數量',
            name: 'QTY',
            maskRe: /[0-9]/,
            regexText: '只能輸入數字',
            regex: /^[1-9]\d*$/,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '計量單位',
            name: 'UNIT',
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '單價',
            name: 'PRICE',
            maskRe: /[0-9.]/,
            regexText: '只能輸入數字',
            regex: /^[0-9]\d*(\.\d*)?$/,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required',
            validator: function (value) {
                if (value == 0 && (T11Form.getForm().findField('MMCODE').getValue() == null || T11Form.getForm().findField('MMCODE').getValue() == "")) {
                   return '單價不可為0';
                }
                return true;
            }
        }, {
            xtype: 'textarea',
            fieldLabel: '備考',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            xtype: 'filegrid',
            fieldLabel: '附加檔案',
            name: 'UK',
            width: '100%',
            height: 100
        }],
        buttons: [{
            itemId: 't11submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    if (T11Form.getForm().findField('QTY').getValue() <= 0)
                        Ext.Msg.alert('提醒', '數量需大於0');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T11Submit();
                            }
                        }
                        );
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't11cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid() ) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T11Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T11Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            T12Load();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            T12Load();
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            T12Load();
                            break;
                    }
                    T11Cleanup();
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
    function T11Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t11Grid').unmask();
        var f = T11Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T11Form.down('#t11cancel').hide();
        T11Form.down('#t11submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT11a();
    }

    var T12Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'displayfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '申請單號',
            name: 'DN',
            xtype: 'hidden'
        }, {
            fieldLabel: '院內碼',
            name: 'MMCODE'
        }, {
            fieldLabel: '品名及規格廠牌',
            name: 'NMSPEC'
        }, {
            fieldLabel: '計量單位',
            name: 'UNIT'
        }, {
            fieldLabel: '單價',
            name: 'PRICE'
        }, {
            fieldLabel: '單品需求總價',
            name: 'SUMPRICE'
        }],
        buttons: [{
            itemId: 't11submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    if (T11Form.getForm().findField('QTY').getValue() <= 0)
                        Ext.Msg.alert('提醒', '數量需大於0');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T11Submit();
                            }
                        }
                        );
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't11cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    var callableWin = null;
    popRejectReason = function () {
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'rejectreason',
                height: '95%',
                layout: 'fit',
                closable: false,
                items: [
                    {
                        xtype: 'fieldset',
                        title: '請輸入【剔退原因】',
                        autoHeight: true,
                        style: "margin:5px;background-color: #ecf5ff;",
                        //cls: 'fieldset-title-bigsize',
                        width: 250,
                        height: 150,
                        layout: 'anchor',
                        items: [
                            {
                                xtype: 'container',
                                layout: {
                                    type: 'table',
                                    columns: 1
                                },
                                items: [
                                    {
                                        xtype: 'textarea',
                                        id: 'REASON',
                                        width: 250,
                                        fieldCls: 'required',
                                        allowBlank: false,
                                        enforceMaxLength: true,
                                        maxLength: 100
                                    }
                                ]
                            }
                        ]
                    }
                ],
                buttons: [{
                    id: 'reject',
                    disabled: false,
                    margin: '0 5 30 0',
                    text: '剔退',
                    handler: function () {
                        if (popform.getForm().findField('REASON').isValid()) {
                            Ext.MessageBox.confirm('剔退', '是否確定剔退?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Set = '/api/BC0003/MasterReject';
                                    var reasonValue = popform.getForm().findField('REASON').getValue();
                                    T1Form.getForm().findField('REASON').setValue(reasonValue);
                                    T1Form.getForm().findField('x').setValue('R');
                                    T1Submit();
                                }
                            });
                        }
                        else
                            Ext.Msg.alert('訊息', '剔退原因為必填');
                    }
                }, {
                    id: 'winclosed',
                    disabled: false,
                    margin: '0 20 30 0',
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popform, '剔退', 300, 180);
        }
        callableWin.show();
    }

    function getINID(parTuser) {
        Ext.Ajax.request({
            url: GetInidByTuser,
            method: reqVal_p,
            params: { tuser: parTuser },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T1Form.getForm().findField('APP_INID').setValue(tb_data[0].INID);
                        T1Form.getForm().findField('INID').setValue(tb_data[0].INID);
                        T1Form.getForm().findField('INIDNAME').setValue(tb_data[0].INID_NAME);
                    }
                    else {
                        T1Form.getForm().findField('APP_INID').setValue('');
                        T1Form.getForm().findField('INID').setValue('');
                        T1Form.getForm().findField('INIDNAME').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function chkMMCODE(parMmcode) {
        Ext.Ajax.request({
            url: GetMmdataByMmcode,
            method: reqVal_p,
            params: { mmcode: parMmcode },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T11Form.getForm().findField('CHARGE').setValue(tb_data[0].M_PAYKIND);
                        T11Form.getForm().findField('CHARGE').setReadOnly(true);
                        T11Form.getForm().findField('CHARGE').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('NMSPEC').setValue(tb_data[0].MMNAME);
                        T11Form.getForm().findField('NMSPEC').setReadOnly(true);
                        T11Form.getForm().findField('NMSPEC').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('UNIT').setValue(tb_data[0].BASE_UNIT);
                        T11Form.getForm().findField('UNIT').setReadOnly(true);
                        T11Form.getForm().findField('UNIT').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('PRICE').setValue(tb_data[0].UPRICE);
                        //T11Form.getForm().findField('PRICE').setReadOnly(true);
                    }
                    else {
                        Ext.Msg.alert('訊息', '院內碼不存在,請重新輸入!');
                        T11Form.getForm().findField('MMCODE').setValue('');
                        T11Form.getForm().findField('CHARGE').setValue('');
                        T11Form.getForm().findField('NMSPEC').setValue('');
                        T11Form.getForm().findField('UNIT').setValue('');
                        T11Form.getForm().findField('PRICE').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    //function chkAGENNO(parAgenno) {
    //    Ext.Ajax.request({
    //        url: GetAgennmByAgenno,
    //        method: reqVal_p,
    //        params: { agen_no: parAgenno },
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                var tb_data = data.etts;
    //                if (tb_data.length > 0) {
    //                    T1Form.getForm().findField('AGEN_NAMEC').setValue(tb_data[0].AGEN_NAME);
    //                }
    //                else {
    //                    Ext.Msg.alert('訊息', '廠商代碼不存在,請重新輸入!');
    //                    T1Form.getForm().findField('AGEN_NO').setValue('');
    //                    T1Form.getForm().findField('AGEN_NAMEC').setValue('');
    //                }
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
    //}

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?DN=' + T1LastRec.data.DN + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    // 核可時檢查欄位是否為空白
    function chkFormEmpty() {
        if (T1Form.getForm().findField('DUEDATE').getValue() == '')
            return '申請單[交貨期限]不可空白';
        else if (T1Form.getForm().findField('DELIVERY').getValue() == '')
            return '申請單[交貨地點]不可空白';
        else if (T1Form.getForm().findField('ACCEPT').getValue() == '')
            return '申請單[驗收方式]不可空白';
        else if (T1Form.getForm().findField('PAYWAY').getValue() == '')
            return '申請單[付款方式]不可空白';
        //else if (T1Form.getForm().findField('AGEN_NO').getValue() == '' || T1Form.getForm().findField('AGEN_NO').getValue() == null)
        //    return '申請單[廠商代碼]不可空白';
        for (var i = 0; i < T11Store.getCount(); i++) {
            if (T11Store.getData().items[i].data['MMCODE'] == '')
                return '申請單項次' + T11Store.getData().items[i].data['SEQ'] + '院內碼不可空白';
        }

        return true;
    }

    //***********DetailGrid TAB****************
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "申請明細":
                        break;
                    case "數量彙總":
                        break;
                }
                T11Rec = 0;
                T11LastRec = null;
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            title: '申請明細',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T11Grid]
            }
            ]
        },
        {
            title: '數量彙總',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 't2Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T12Grid]
            }
            ]
        }
        ]
    });


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
                itemId: 'tGrid',
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
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
                            items: [T1Grid]
                        }, {
                            itemId: 't11Grid',
                            region: 'south',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            height: '50%',
                            split: true,
                            //items: [T11Grid]
                            items: [TATabs]
                        }
                    ]
                }]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T11Form, T12Form]
            }
        ]
    });

    //T1Load();
    T1Query.getForm().findField('P1').setValue(new Date().addMonth(-1));
    T1Query.getForm().findField('P2').setValue(new Date());
    T1Query.getForm().findField('P3').setValue('C');
    T1Query.getForm().findField('P0').focus();
    Ext.getCmp('QryButton').click();
});
