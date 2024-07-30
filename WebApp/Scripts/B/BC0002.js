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
    //var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var GetMmdataByMmcode = '/api/BC0002/GetMmdataByMmcode';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var MmcodeComboGet = '/api/BC0002/GetMmcodeCombo';
    var T1Name = "小額採購申請";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var NRS = "N";
    if (Ext.getUrlParam('NRS') != null) NRS = Ext.getUrlParam('NRS');

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

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
            border: false,
            layout: 'hbox',
            items: [{
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
                text: '查詢',
                handler: function () {
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                    T1Load();
                    msglabel('訊息區:');

                    T11Grid.columns[2].setVisible(false);
                    T11Grid.columns[3].setVisible(false);
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

    var T1Store = Ext.create('WEBAPP.store.PhSmallM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: session['UserId'],
                    p5: 'New'
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

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BC0002/MasterCreate';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BC0002/MasterUpdate';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BC0002/MasterDelete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                itemId: 't1audit', text: '送審', disabled: true, handler: function () {

                    Ext.Ajax.request({
                        url: '/api/BC0002/DetailCount',
                        method: reqVal_p,
                        params: {
                            dn: T1LastRec.data['DN']
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.afrs == 0) {
                                Ext.Msg.alert('提醒', '尚未輸入資料, 不可以送審');
                                return;
                            }


                            if (NRS == "Y") {  // 針對護理部不提供選擇簽審主管
                                Ext.MessageBox.confirm('送審', '是否要送出申請單？', function (btn, text) {
                                    if (btn === 'yes') {
                                        T1Set = '/api/BC0002/MasterAudit';
                                        T1Form.getForm().findField('x').setValue('A');
                                        T1Form.getForm().findField('NRSFLAG').setValue(NRS);
                                        T1Submit();
                                    }
                                }
                                );
                            } else {
                                T4Query.getForm().findField('BOSS').setValue('');
                                T4Query.getForm().findField('QRY').setValue('N');
                                T4Load();
                                showWin4();
                            }
                            
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
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });
                    
                }
            }
        ]
    });
    Ext.define('T4Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'BOSS_INID', type: 'string' },
            { name: 'BOSS_NAME', type: 'string' },
            { name: 'BOSS_ID', type: 'string' }
        ]
    });
    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        autoLoad: true,
        sorters: [{ property: 'BOSS_NAME', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BC0002/GetBOSS',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    BOSS: T4Query.getForm().findField('BOSS').getValue(),
                    QRY: T4Query.getForm().findField('QRY').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })
    var T4Tool = Ext.create('Ext.PagingToolbar', {
        store: T4Store,
        displayInfo: true,
        border: false,
        plain: true,
        //pageSize: 20, 設在 store
        displayMsg: "顯示第 {0} 筆到第 {1} 筆,共 {2} 筆",
        emptyMsg: "沒有資料"
    });
    function T4Load() {
        T4Tool.moveFirst();
        //T4Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }
    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            xtype: 'textfield',
            fieldLabel: '',
            name: 'QRY',
            value: 'N',
            hidden: true
        }, {
            xtype: 'textfield',
            fieldLabel: '主管姓名',
            name: 'BOSS',
            enforceMaxLength: true,
            labelWidth: 60,
            width: 170,
            padding: '0 4 0 4',
            value: 'xx'
        }, {
            xtype: 'button',
            text: '查詢',
            id: 'T4btn1',
            handler: function () {
                T4Query.getForm().findField("QRY").setValue("Y");
                T4Store.load({
                    params: {
                        start: 0
                    }
                });
            }
        }, {
            xtype: 'button', text: '確定',
            id: 'T4btn2',
            handler: function () {
                T4GridSelectionCheck();
                //var selection = T4Grid.getSelection();
                //if (selection.length == 0) {
                //    Ext.Msg.alert('提醒', '請勾選項目');
                //}
                //else {
                //    let bossname = '';
                //    let bossid = '';
                //    $.map(selection, function (item, key) {
                //        bossname = item.get('BOSS_NAME');
                //        bossid = item.get('BOSS_ID');
                //    })
                //    Ext.MessageBox.confirm('送審', '審核主管為' + bossname + '是否正確？', function (btn, text) {
                //        if (btn === 'yes') {
                //            T1Set = '/api/BC0002/MasterAudit';
                //            T1Form.getForm().findField('x').setValue('A');
                //            T1Form.getForm().findField('NRSFLAG').setValue(NRS);
                //            T1Form.getForm().findField('NEXT_USER').setValue(bossid);
                //            T1Submit();
                //        }
                //    });
                //}
            }
        }, {
            xtype: 'button',
            text: '關閉',
            handler: function () {
                win4.hide();
            }
        }
        ]
    });
    function T4GridSelectionCheck() {
        var selection = T4Grid.getSelection();
        if (selection.length != 0) {
            let bossname = '';
            let bossid = '';
            $.map(selection, function (item, key) {
                bossname = item.get('BOSS_NAME');
                bossid = item.get('BOSS_ID');
            })
            Ext.MessageBox.confirm('送審', '審核主管為<font color=blue> ' + bossname + ' </font>是否正確？', function (btn, text) {
                if (btn === 'yes') {
                    T1Set = '/api/BC0002/MasterAudit';
                    T1Form.getForm().findField('x').setValue('A');
                    T1Form.getForm().findField('NRSFLAG').setValue(NRS);
                    T1Form.getForm().findField('NEXT_USER').setValue(bossid);
                    T1Submit();
                }
            });
        }
    }
    var T4Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T4Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T4Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SINGLE',
            listeners: {
                selectionchange: function (model, record, index, eOpts) {
                    T4GridSelectionCheck();
                }
            }
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "責任中心",
            dataIndex: 'BOSS_INID',
            width: 100,
            sortable: true
        }, {
            text: "姓 名",
            dataIndex: 'BOSS_NAME',
            width: 120,
            sortable: true
        }, {
            text: "BOSS_ID",
            dataIndex: 'BOSS_ID',
            hidden: true
        }, {
            header: "",
            flex: 1
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
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhSmallM');
            T1Form.loadRecord(r);
            f.clearInvalid();
            f.findField('DN').setValue('系統自編');
            f.findField('APP_USER').setValue(session['UserName']);
            getINID(session['UserId']);
            u = f.findField('USEWHERE');
        }
        else {
            u = f.findField('USEWHERE');
        }
        f.findField('x').setValue(x);
        f.findField('ALT').setReadOnly(false);
        f.findField('DEMAND').setReadOnly(false);
        f.findField('TEL').setReadOnly(false);
        f.findField('USEWHEN').setReadOnly(false);
        f.findField('USEWHERE').setReadOnly(false);
        f.findField('OTHERS').setReadOnly(false);
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
                    T11Set = '/api/BC0002/DetailCreate';
                    setFormT11('I', '新增');
                }
            },
            {
                itemId: 't11edit', text: '修改', disabled: true, handler: function () {
                    T11Set = '/api/BC0002/DetailUpdate';
                    setFormT11("U", '修改');
                }
            }, {
                itemId: 't11delete', text: '刪除', disabled: true,
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
            },
            {
                xtype: 'filefield',
                name: 'filefield',
                id: 'filefield',
                buttonOnly: true,
                buttonText: '選擇檔案檢核',
                width: 90,
                disabled: true,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //var newValue = value.replace(/C:\\fakepath\\/g, '');
                        //widget.setRawValue(newValue);
                        //Ext.getCmp('check').setDisabled(true);
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            T11Gird.getForm().findField('filefield').fileInputEl.dom.value = '';
                            msglabel("訊息區:");
                        } else {
                            //Ext.getCmp('check').setDisabled(false);
                            msglabel("檢核檔案中");

                            Ext.getCmp('import').setDisabled(true);
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/BC0002/CheckExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        T11Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");

                                        //Ext.getCmp('import').setDisabled(true);
                                    }
                                    else {
                                        msglabel("訊息區:檔案檢核成功");
                                        T11Store.loadData(data.etts, false);
                                        //IsSend = true;

                                        if (data.msg == 'OK') {
                                            Ext.getCmp('import').setDisabled(false);
                                        }

                                        T11Grid.columns[2].setVisible(true);
                                        T11Grid.columns[3].setVisible(true);
                                    }

                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '匯入',
                id: 'import',
                name: 'import',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯入', '匯入將刪除現有明細資料，確認匯入?', function (btn, text) {
                        if (btn === 'yes') {


                            Ext.getCmp('import').setDisabled(true);
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            msglabel("訊息區:匯入中");
                            Ext.Ajax.request({
                                url: '/api/BC0002/Import',
                                method: reqVal_p,
                                params: {
                                    data: Ext.encode(Ext.pluck(T11Store.data.items, 'data')),
                                    DN: T1LastRec.data['DN']
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.MessageBox.alert("提示", "匯入完成");
                                        Ext.getCmp('import').setDisabled(true);
                                        T11Store.loadData(data.etts, false);
                                        msglabel("訊息區:匯入完成");
                                    }
                                    myMask.hide();
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
                                            Ext.Msg.alert('失敗', "匯入失敗");
                                            break;
                                    }
                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                xtype: 'docbutton',
                text: '範例文件下載',
                documentKey: 'BC0002'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '',
                value: '<span style="color:red">檢核成功需點匯入才會儲存資料</span>'
            }
        ]
    });
    function setFormT11(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T11Form.getForm();
        T1Form.setVisible(false);
        T11Form.setVisible(true);
        f.findField('UK').setReadOnly(false);
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhSmallD');
            T11Form.loadRecord(r);
            f.clearInvalid();
            f.findField('SEQ').setValue('系統自編');
            f.findField('DN').setValue(T1LastRec.data['DN']);
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
            f.findField('PRICE').setReadOnly(true);
            f.findField('PRICE').setFieldStyle("background: #CCCCCC");
            f.findField('PRICE').clearInvalid();
        }
        f.findField('x').setValue(x);
        f.findField('MEMO').setReadOnly(false);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('INID').setReadOnly(false);
        f.findField('QTY').setReadOnly(false);
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
            width: 160
        }, {
            xtype: 'datecolumn',
            text: "申請日期",
            dataIndex: 'APPTIME',
            format: 'Xmd',
            width: 80
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
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
                T11Load();

                Ext.getCmp('import').setDisabled(true);
                Ext.getCmp('filefield').fileInputEl.dom.value = '';


                T11Grid.columns[2].setVisible(false);
                T11Grid.columns[3].setVisible(false);
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
                // 非新增/剔退的資料就不允許修改/刪除
                T1Grid.down('#t1edit').setDisabled(true);
                T1Grid.down('#t1delete').setDisabled(true);
                T1Grid.down('#t1audit').setDisabled(true);
                T11Grid.down('#t11add').setDisabled(true);
                Ext.getCmp('filefield').setDisabled(true);
            }
            else {
                T1Grid.down('#t1audit').setDisabled(false);
                T11Grid.down('#t11add').setDisabled(false);

                Ext.getCmp('filefield').setDisabled(false);

            }
        }
        else {
            T1Form.getForm().reset();
            T1Grid.down('#t1audit').setDisabled(true);
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
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 200,
            hidden: true
        }, {
            text: "異動結果",
            dataIndex: 'IMPORT_RESULT',
            width: 100,
            hidden: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "",
            dataIndex: 'OLD_MMCODE',
            hidden:true
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
            text: "",
            dataIndex: 'OLD_NMSPEC',
            hidden: true
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
                    }
                }
            },
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT11a();
            }
        }
    });
    function setFormT11a() {
        T11Grid.down('#t11edit').setDisabled(T11Rec === 0);
        T11Grid.down('#t11delete').setDisabled(T11Rec === 0);
        if (T11LastRec && (T11LastRec.data['IMPORT_RESULT'] == null || T11LastRec.data['IMPORT_RESULT'] == "匯入成功")) {
            isNew = false;
            T11Form.loadRecord(T11LastRec);
            T11Form.getForm().findField('CHARGE').setValue(T11LastRec.data['CHARGE'].split(' ')[0]);
            T11Form.getForm().findField('INID').setValue(T11LastRec.data['INID'].split(' ')[0]);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            if (T1LastRec.data['STATUS'].split(' ')[0] != 'A' && T1LastRec.data['STATUS'].split(' ')[0] != 'D') {
                // 主檔非新增/剔退的資料就不允許修改/刪除
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
            fieldLabel: 'NEXT_USER',
            name: 'NEXT_USER',
            xtype: 'hidden'
        }, {
            xtype: 'hiddenfield',
            fieldLabel: '護理部識別',
            name: 'NRSFLAG',
            submitValue: true,
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
            xtype: 'textarea',
            fieldLabel: '其他',
            name: 'OTHERS',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
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
            xtype: 'displayfield',
            fieldLabel: '剔退說明',
            name: 'REASON',
            readOnly: true,
            submitValue: true,
            renderer: function (val, meta, record) {
                return '<font color="red">' + val + '</font>';
            }
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '交貨期限',
            //    name: 'DUEDATE',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    readOnly: true
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '交貨地點',
            //    name: 'DELIVERY',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    readOnly: true
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '驗收方式',
            //    name: 'ACCEPT',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    readOnly: true
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '付款方式',
            //    name: 'PAYWAY',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    readOnly: true
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '廠商代碼',
            //    name: 'AGEN_NO',
            //    enforceMaxLength: true,
            //    maxLength: 6,
            //    readOnly: true
            //}, {
            //    xtype: 'displayfield',
            //    fieldLabel: '廠商名稱',
            //    name: 'AGEN_NAMEC',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    readOnly: true
        }, {
            xtype: 'hiddenfield',
            fieldLabel: '申請單位審核主管',
            name: 'APP_USER1',
            submitValue: true
        }, {
            xtype: 'textarea',
            fieldLabel: '簽審歷程',
            name: 'SIGNDATA',
            enforceMaxLength: true,
            readOnly: true
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
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.DN);
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
                            msglabel('訊息區:資料刪除成功');
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            if (NRS != "Y")
                                win4.hide();
                            T11Grid.down('#t11add').setDisabled(true);
                            T11Grid.down('#t11edit').setDisabled(true);
                            T11Grid.down('#t11delete').setDisabled(true);
                            msglabel('訊息區:送審完成');
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
                //if (f.findField('MMCODE').getValue() != '' && f.findField('MMCODE').getValue() != null && f.findField('MMCODE').readOnly == false)
                //    chkMMCODE(f.findField('MMCODE').getValue());
                if (f.findField('MMCODE').getValue() == null || f.findField('MMCODE').getValue() == "") {
                    f.findField('CHARGE').setReadOnly(false);
                    f.findField('CHARGE').setFieldStyle("background: pink;");
                    f.findField('NMSPEC').setReadOnly(false);
                    f.findField('NMSPEC').setFieldStyle("background: pink;");
                    f.findField('UNIT').setReadOnly(false);
                    f.findField('UNIT').setFieldStyle("background: pink;");
                    f.findField('PRICE').setReadOnly(false);
                    f.findField('PRICE').setFieldStyle("background: pink;");
                    f.findField('CHARGE').setValue('');
                    f.findField('NMSPEC').setValue('');
                    f.findField('UNIT').setValue('');
                    f.findField('PRICE').setValue('');
                    f.findField('QTY').setValue('');
                }
                else {
                    f.findField('CHARGE').setReadOnly(true);
                    f.findField('CHARGE').setFieldStyle("background: #CCCCCC"); //淺灰
                    f.findField('NMSPEC').setReadOnly(true);
                    f.findField('NMSPEC').setFieldStyle("background: #CCCCCC");
                    f.findField('UNIT').setReadOnly(true);
                    f.findField('UNIT').setFieldStyle("background: #CCCCCC");
                    f.findField('PRICE').setReadOnly(true);
                    f.findField('PRICE').setFieldStyle("background: #CCCCCC");
                    f.findField('PRICE').clearInvalid();
                }
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
        limit: 50,
        queryUrl: InidComboGet,
        //storeAutoLoad: true,
        insertEmptyRow: true,
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        maxLength: 6,
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
            fieldLabel: '需求單位名稱',
            name: 'INIDNAME',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            submitValue: false
        }, {
            fieldLabel: '數量',
            name: 'QTY',
            maskRe: /[0-9]/,
            regexText: '只能輸入數字',
            regex: /^[1-9]\d*$/,
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, T11FormCharge,
        {
            fieldLabel: '品名及規格廠牌',
            name: 'NMSPEC',
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '計量單位',
            name: 'UNIT',
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '最小撥補量',
            name: 'TOT_DISTUN',
            xtype:'displayfield'
        }, {
            fieldLabel: '單價',
            name: 'PRICE',
            maskRe: /[0-9.]/,
            regexText: '只能輸入數字',
            regex: /^[0-9]\d*(\.\d*)?$/,
            //readOnly: true,
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
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            if (T1LastRec) {
                                T11Form.getForm().findField('DN').setValue(T1LastRec.data['DN']);
                                T11Submit();
                            }
                            else {
                                T11Load();
                            }
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
            itemId: 't11cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid()) {
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
                            //var v = action.result.etts[0];
                            //r.set(v);
                            //T11Store.insert(0, r);
                            //r.commit();
                            T11Store.load();
                            T11Form.getForm().findField('INID').setValue('');
                            T11Form.getForm().findField('INIDNAME').setValue('');
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            T11Cleanup();
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
                            T11Cleanup();
                            break;
                    }
                    // T11Cleanup();
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
                        //T11Form.getForm().findField('INID').setValue(tb_data[0].INID);
                        //T11Form.getForm().findField('INIDNAME').setValue(tb_data[0].INID_NAME);
                    }
                    else {
                        T1Form.getForm().findField('APP_INID').setValue('');
                        //T11Form.getForm().findField('INID').setValue('');
                        //T11Form.getForm().findField('INIDNAME').setValue('');
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
                       // T11Form.getForm().findField('CHARGE').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('NMSPEC').setValue(tb_data[0].MMNAME);
                        T11Form.getForm().findField('NMSPEC').setReadOnly(true);
                       // T11Form.getForm().findField('NMSPEC').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('UNIT').setValue(tb_data[0].BASE_UNIT);
                        T11Form.getForm().findField('UNIT').setReadOnly(true);
                       // T11Form.getForm().findField('UNIT').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('PRICE').setValue(tb_data[0].UPRICE);
                        T11Form.getForm().findField('PRICE').setReadOnly(true);
                        // T11Form.getForm().findField('PRICE').setFieldStyle("background: #CCCCCC"); //淺灰
                        T11Form.getForm().findField('TOT_DISTUN').setValue(tb_data[0].TOT_DISTUN)
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
    var winActWidth = viewport.width - 300;
    var winActHeight = viewport.height - 100;
    var win4;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '選擇簽審主管',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T4Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }
    function T4Cleanup() {
        T4Query.getForm().findField("BOSS").setValue("");
        T4Load();
        msglabel('訊息區:');
    }
    function showWin4() {
        if (win4.hidden) {
            T4Cleanup();
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
            T4Cleanup();
        }
    }
    // T1Load();
    // 在這裡用setValue方式填預設值,填入的值可按清除鈕清除
    // 在field設定value的方式指定預設值,按清除鈕時會重置為value的值
    T1Query.getForm().findField('P1').setValue(new Date().addMonth(-1));
    T1Query.getForm().findField('P2').setValue(new Date());
    T1Query.getForm().findField('P0').focus();
});