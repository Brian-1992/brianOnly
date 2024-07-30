Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T11Set = '';
    var GetInidByTuser = '/api/AB0041/GetInidByTuser';
    //var GetAgennmByAgenno = '/api/AB0041/GetAgennmByAgenno';
    var GetMmdataByMmcode = '/api/AB0041/GetMmdataByMmcode';
    var InidComboGet = '/api/AB0041/GetInidCombo';
    var MmcodeComboGet = '/api/AB0041/GetMmcodeCombo';
    var T1Name = "特殊調配配方";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var T11MMCode = null;

    //Master的查詢模式 true為完全符合的查詢 false為後半模糊的查詢
    var query = false;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    /*
    var cb = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'P3',
        id: 'P3',
        fieldLabel: '狀態碼',
        queryParam: {
            GRP_CODE: 'PH_SMALL_M',
            DATA_NAME: 'STATUS'
        },
        width: 170,
        labelWidth: 60,
        insertEmptyRow: true,
        autoSelect: true,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    */
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
            border: false,
            layout: 'hbox',
            items: [{
                xtype: 'textfield',
                fieldLabel: '特殊調配配方碼',
                name: 'P0',
                id: 'P0',
                width: 200,
                labelWidth: 100,
                enforceMaxLength: true,
                maxLength: 22
            },
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                    query = false;
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
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.ME_MDFM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: query
                    //p1: T1Query.getForm().findField('P1').getValue(),
                    //p2: T1Query.getForm().findField('P2').getValue(),
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    //p4: session['UserId'],
                    //p5: 'New'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }
    var T11Store = Ext.create('WEBAPP.store.ME_MDFD', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Form.getForm().findField('MDFM').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T11Load() {
        T11Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AB0041/MasterCreate';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AB0041/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0041/MasterDelete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            //{
            //    itemId: 't1audit', text: '送審', disabled: true, handler: function () {
            //        if (T11Store.getCount() > 0) {
            //            Ext.MessageBox.confirm('送審', '是否要送出申請單？', function (btn, text) {
            //                if (btn === 'yes') {
            //                    T1Set = '/api/AB0041/MasterAudit';
            //                    T1Form.getForm().findField('x').setValue('A');
            //                    T1Submit();
            //                }
            //            }
            //            );
            //        }
            //        else
            //            Ext.Msg.alert('訊息', '尚未輸入資料, 不可以送審');
            //    }
            //}
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_MDFM');
            T1Form.loadRecord(r);
            f.clearInvalid();
            //f.findField('DN').setValue('系統自編');
            //f.findField('APP_USER').setValue(session['UserName']);
            //getINID(session['UserId']);
            //u = f.findField('USEWHERE');
            u = f.findField('MDFM');
            f.findField('MDFM').setReadOnly(false);
        }
        else {
            //u = f.findField('USEWHERE');
            u = f.findField('MDFM');
        }
        f.findField('x').setValue(x);
        f.findField('MD_NAME').setReadOnly(false);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('MDFM_QTY').setReadOnly(false);
        f.findField('MDFM_UNIT').setReadOnly(false);
        f.findField('USE_QTY').setReadOnly(false);
        f.findField('PRESERVE_DAYS').setReadOnly(false);
        f.findField('OPERATION').setReadOnly(false);
        f.findField('ELEMENTS').setReadOnly(false);
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
                itemId: 't11add', text: '新增', disabled: true, handler: function () {
                    T11Set = '/api/AB0041/DetailCreate';
                    setFormT11('I', '新增');
                }
            },
            {
                itemId: 't11edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '/api/AB0041/DetailUpdate';
                    setFormT11("U", '修改');
                }
            }, {
                itemId: 't11delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Set = '/api/AB0041/DetailDelete';
                            T11Form.getForm().findField('x').setValue('D');
                            T11Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT11(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T11Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_MDFD');
            T11Form.loadRecord(r);
            f.clearInvalid();
            //f.findField('SEQ').setValue('系統自編');
            //f.findField('DN').setValue(T1LastRec.data['DN']);
            u = f.findField("MMCODE");
        }
        else {
            u = f.findField('MMCODE');
            T11MMCode = u.getValue();
        }
        T11Form.getForm().findField('MDFM').setValue(T1LastRec.data['MDFM']);
        f.findField('x').setValue(x);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('MMNAME_E').setReadOnly(false);
        f.findField('MDFD_QTY').setReadOnly(false);
        f.findField('MDFD_UNIT').setReadOnly(false);
        f.findField('USE_QTY').setReadOnly(false);
        T11Form.down('#t11cancel').setVisible(true);
        T11Form.down('#t11submit').setVisible(true);
        u.focus();
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
        },
        {
            text: "配方代碼",
            dataIndex: 'MDFM',
            width: 100
        },
        {
            text: "配方名稱",
            dataIndex: 'MD_NAME',
            width: 150
        },
        {
            text: "成品代碼",
            dataIndex: 'MMCODE',
            width: 100
        },
        {
            text: "英文名稱",
            dataIndex: 'MMNAME_E',
            width: 350
        },
        {
            text: "調配劑量",
            dataIndex: 'MDFM_QTY',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 80
        },
        {
            text: "劑量單位",
            dataIndex: 'MDFM_UNIT',
            width: 80
        },
        {
            text: "總量",
            dataIndex: 'USE_QTY',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
            text: "有效天數",
            dataIndex: 'PRESERVE_DAYS',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 80
        },
        {
            text: "操作說明",
            dataIndex: 'OPERATION',
            width: 200
        },
        {
            text: "配置處方",
            dataIndex: 'ELEMENTS',
            width: 130
        },
        {
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
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //setFormT1a();
                T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
                T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
                T11Grid.down('#t11add').setDisabled(T1Rec === 0);
                if (T1LastRec) {
                    isNew = false;
                    T1Form.loadRecord(T1LastRec);
                    var f = T1Form.getForm();
                    f.findField('x').setValue('U');
                    if (T1LastRec.data['STATUS'] != 'D') {
                        // 非新增/剔退的資料就不允許修改/刪除
                        //T1Grid.down('#t1edit').setDisabled(true);
                        //T1Grid.down('#t1delete').setDisabled(true);
                        //T1Grid.down('#t1audit').setDisabled(true);
                        //T11Grid.down('#t11add').setDisabled(true);
                    }
                    else {
                        //T1Grid.down('#t1audit').setDisabled(false);
                        //T11Grid.down('#t11add').setDisabled(false);
                    }
                }
                else {
                    T1Form.getForm().reset();
                    //T1Grid.down('#t1audit').setDisabled(true);
                    //T11Grid.down('#t11add').setDisabled(true);
                    //T11Grid.down('#t11edit').setDisabled(true);
                    //T11Grid.down('#t11delete').setDisabled(true);
                }
                T11Load();
            }
        }
    });
    /*
    function setFormT1a() {
        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        T11Grid.down('#t11add').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            //T1Form.getForm().findField('CHARGE').setValue(T1LastRec.data['CHARGE'].split(' ')[0]);
            //T1Form.getForm().findField('INID').setValue(T1LastRec.data['INID'].split(' ')[0]);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            //if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
            if (T1LastRec.data['STATUS'] != 'D') {
                // 非新增/剔退的資料就不允許修改/刪除
                T1Grid.down('#t1edit').setDisabled(true);
                T1Grid.down('#t1delete').setDisabled(true);
                //T1Grid.down('#t1audit').setDisabled(true);
                T11Grid.down('#t11add').setDisabled(true);
            }
            else {
                //T1Grid.down('#t1audit').setDisabled(false);
                T11Grid.down('#t11add').setDisabled(false);
            }
        }
        else {
            //T1Form.getForm().reset();
            //T1Grid.down('#t1audit').setDisabled(true);
            T11Grid.down('#t11add').setDisabled(true);
            T11Grid.down('#t11edit').setDisabled(true);
            T11Grid.down('#t11delete').setDisabled(true);
        }
    }*/

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
        },
        {
            xtype: 'hidden',
            text: "配方代碼",
            dataIndex: 'MDFM',
            width: 100
        },
        {
            text: "成品代碼",
            dataIndex: 'MMCODE',
            width: 100
        },
        {
            text: "英文名稱",
            dataIndex: 'MMNAME_E',
            width: 350
        },
        {
            text: "調配劑量",
            dataIndex: 'MDFD_QTY',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 80
        },
        {
            text: "劑量單位",
            dataIndex: 'MDFD_UNIT',
            width: 80
        },
        {
            text: "總量",
            dataIndex: 'USE_QTY',
            align: 'right', // Right align the contents
            style: 'text-align:left', // Keep left align for Header
            width: 100
        },
        {
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
                    }
                }
            },
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                //setFormT11a();
                T11Grid.down('#t11add').setDisabled(T11Rec === 0);
                T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
                T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
                if (T11LastRec) {
                    isNew = false;
                    T11Form.loadRecord(T11LastRec);
                    var f = T11Form.getForm();
                    f.findField('x').setValue('U');
                    //if (T1LastRec.data['STATUS'] != 'D') {
                    //    // 主檔非新增/剔退的資料就不允許修改/刪除
                    //    //T11Grid.down('#t11edit').setDisabled(true);
                    //    //T11Grid.down('#t11delete').setDisabled(true);
                    //}
                }
                //else {
                //    T11Form.getForm().reset();
                //}
            }
        }
    });
    /*
    function setFormT11a() {
        T11Grid.down('#t11add').setDisabled(T11Rec === 0);
        T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T11LastRec) {
            isNew = false;
            T11Form.loadRecord(T11LastRec);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
                // 主檔非新增/剔退的資料就不允許修改/刪除
                T11Grid.down('#t11edit').setDisabled(true);
                T11Grid.down('#t11delete').setDisabled(true);
            }

        }
        else {
            T11Form.getForm().reset();
        }
    }
    */
    var T1FormCharge = Ext.create('WEBAPP.form.ParamCombo', {
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
    var T1FormInid = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '需求單位代碼',
        limit: 20,
        queryUrl: InidComboGet,
        //storeAutoLoad: true,
        insertEmptyRow: true,
        readOnly: true,
        listeners: {
            select: function (c, r, i, e) {
                T1Form.getForm().findField('INIDNAME').setValue(r.get('INID_NAME'));
            }
        }
    });
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
            fieldLabel: '配方代碼',
            name: 'MDFM',
            enforceMaxLength: true,
            maxLength: 8,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '配方名稱',
            name: 'MD_NAME',
            enforceMaxLength: true,
            maxLength: 40,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '成品代碼',
            name: 'MMCODE',
            enforceMaxLength: true,
            maxLength: 13,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文名稱',
            name: 'MMNAME_E',
            enforceMaxLength: true,
            maxLength: 200,
            readOnly: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '調配劑量',
            name: 'MDFM_QTY',
            enforceMaxLength: true,
            maxLength: 11,
            allowBlank: false,
            fieldCls: 'required',
            minValue: 0,
            readOnly: true
        }, {
            fieldLabel: '劑量單位',
            name: 'MDFM_UNIT',
            enforceMaxLength: true,
            maxLength: 6,
            allowBlank: false,
            fieldCls: 'required',
            minValue: 0,
            readOnly: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '總量',
            name: 'USE_QTY',
            enforceMaxLength: true,
            maxLength: 16,
            allowBlank: false,
            fieldCls: 'required',
            minValue: 0,
            readOnly: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '有效天數',
            name: 'PRESERVE_DAYS',
            enforceMaxLength: true,
            maxLength: 38,
            allowBlank: false,
            fieldCls: 'required',
            minValue: 0,
            readOnly: true
        }, {
            xtype: 'textarea',
            fieldLabel: '操作說明',
            name: 'OPERATION',
            enforceMaxLength: true,
            maxLength: 400,
            readOnly: true
        }, {
            xtype: 'textarea',
            fieldLabel: '配置處方',
            name: 'ELEMENTS',
            enforceMaxLength: true,
            maxLength: 400,
            readOnly: true
        },],
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
                    Ext.Msg.alert('提醒', '請輸入必填欄位資料(數字欄位必須大於0)');
                    msglabel('訊息區:請輸入必填欄位資料(數字欄位必須大於0)');
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
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.MDFM);
                            query = true;
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();

                            T1Load();
                            msglabel('訊息區:資料刪除成功');
                            break;
                            //case "A":
                            //    var v = action.result.etts[0];
                            //    r.set(v);
                            //    r.commit();
                            //    T11Grid.down('#t11add').setDisabled(true);
                            //    T11Grid.down('#t11edit').setDisabled(true);
                            //    T11Grid.down('#t11delete').setDisabled(true);
                            //    msglabel('訊息區:送審完成');
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
        //setFormT1a();
        //T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        //T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        //T11Grid.down('#t11add').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'] != 'D') {
                // 非新增/剔退的資料就不允許修改/刪除
                //T1Grid.down('#t1edit').setDisabled(true);
                //T1Grid.down('#t1delete').setDisabled(true);
                //T11Grid.down('#t11add').setDisabled(true);
            }
            else {
                //T11Grid.down('#t11add').setDisabled(false);
            }
        }
        else {
            T1Form.getForm().reset();
            //T11Grid.down('#t11add').setDisabled(true);
            //T11Grid.down('#t11edit').setDisabled(true);
            //T11Grid.down('#t11delete').setDisabled(true);
        }
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
            blur: function (field, eve, eOpts) {
                var chkType = T1Form.getForm().findField('x').getValue();
                if (field.getValue() != '' && field.getValue() != null
                    && field.readOnly == false)
                    chkMMCODE(field.getValue());
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
            xtype: 'hidden',
            fieldLabel: '配方代碼',
            name: 'MDFM',
            readOnly: true
        }, {
            fieldLabel: '成品代碼',
            name: 'MMCODE',
            maxLength: 8,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文名稱',
            name: 'MMNAME_E',
            readOnly: true,
            allowBlank: false,
        }, {
            xtype: 'numberfield',
            fieldLabel: '調配劑量',
            name: 'MDFD_QTY',
            maxLength: 11,
            readOnly: true,
            allowBlank: false,
            minValue: 0,
            fieldCls: 'required'
        }, {
            fieldLabel: '劑量單位',
            name: 'MDFD_UNIT',
            maxLength: 6,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'numberfield',
            fieldLabel: '總量',
            name: 'USE_QTY',
            maxLength: 16,
            readOnly: true,
            allowBlank: false,
            minValue: 0,
            fieldCls: 'required'
        }],
        buttons: [{
            itemId: 't11submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T11Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '請輸入必填欄位資料(數字欄位必須大於0)');
                    msglabel('訊息區:請輸入必填欄位資料(數字欄位必須大於0)');
                }
            }
        }, {
            itemId: 't11cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                params: {
                    MMCODE_DETAIL: T11MMCode
                },
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
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
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
        //setFormT11a();
        //T11Grid.down('#t11add').setDisabled(T11Rec === 0);
        //T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        //T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T11LastRec) {
            isNew = false;
            T11Form.loadRecord(T11LastRec);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            //    if (T1LastRec.data['STATUS'] != 'D') {
            //        // 主檔非新增/剔退的資料就不允許修改/刪除
            //        //T11Grid.down('#t11edit').setDisabled(true);
            //        //T11Grid.down('#t11delete').setDisabled(true);
            //    }
            //}
            //else {
            //    T11Form.getForm().reset();
        }
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
    //                else
    //                {
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

    //function chkMMCODE(parMmcode) {
    //    Ext.Ajax.request({
    //        url: GetMmdataByMmcode,
    //        method: reqVal_p,
    //        params: { mmcode: parMmcode },
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                var tb_data = data.etts;
    //                if (tb_data.length > 0) {
    //                    T11Form.getForm().findField('NMSPEC').setValue(tb_data[0].MMNAME);
    //                    T11Form.getForm().findField('UNIT').setValue(tb_data[0].BASE_UNIT);
    //                    T11Form.getForm().findField('PRICE').setValue(tb_data[0].M_CONTPRICE);
    //                }
    //                else {
    //                    Ext.Msg.alert('訊息', '院內碼不存在,請重新輸入!');
    //                    msglabel('訊息區:院內碼不存在,請重新輸入!');
    //                    T11Form.getForm().findField('MMCODE').setValue('');
    //                    T11Form.getForm().findField('NMSPEC').setValue('');
    //                    T11Form.getForm().findField('UNIT').setValue('');
    //                    T11Form.getForm().findField('PRICE').setValue('');
    //                }
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
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
                            items: [T11Grid]
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
                items: [T1Form, T11Form]
            }
        ]
    });

    // T1Load();
    // 在這裡用setValue方式填預設值,填入的值可按清除鈕清除
    // 在field設定value的方式指定預設值,按清除鈕時會重置為value的值
    //T1Query.getForm().findField('P1').setValue(new Date().addMonth(-1));
    //T1Query.getForm().findField('P2').setValue(new Date());
    T1Query.getForm().findField('P0').focus();
});
