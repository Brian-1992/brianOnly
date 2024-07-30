Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Get = '/api/AA0076/All'; // 查詢
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "庫房批號效期檔維護";
    var WhnoComboGet = '../../../api/AA0076/GetWhnoCombo';
    var MmcodeComboGet = '../../../api/AA0076/GetMmcodeCombo/';
    var GetMmdataByMmcode = '/api/AA0076/GetMmdataByMmcode';
    var T1GetExcel = '';  //匯出

    var T1Rec = 0;
    var T1LastRec = null;

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 院內碼清單
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 疫苗,非疫苗
    var e_vaccine = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //批號
    var st_LOT_NO = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //效期
    var st_EXP_DATE = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    

    // 庫存零,庫存非零
    var storehourse = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


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

        // 疫苗,非疫苗
        e_vaccine.add({ VALUE: 'Y', TEXT: 'Y 疫苗' });
        e_vaccine.add({ VALUE: 'N', TEXT: 'N 非疫苗' });

        //庫存零,庫存非零
        storehourse.add({ VALUE: '0', TEXT: '零' });
        storehourse.add({ VALUE: '1', TEXT: '非零' });
    }
    setComboData();

    var mLabelWidth = 60;
    var mWidth = 200;

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0076/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                wh_no: T1QueryForm.getForm().findField('P0').getValue(),  //p0:預設是動態MMCODE
            };
        },
        listeners: {
        select: function (oldValue, newValue, eOpts) {
            //選取下拉項目時，顯示回傳值
            if (T1QueryForm.getForm().findField('P0').isValid()) {

                var mmcode = newValue.data.VALUE;
                setLOT_NO(mmcode);

                var f = T1QueryForm.getForm();
                var u = f.findField('P2');
                u.clearValue();
                u.clearInvalid();
            } else {
                Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
            }
            }
        },
    });


    function setLOT_NO(mmcode) {
        Ext.Ajax.request({
            url: '/api/AA0076/Getst_LOT_NO',
            method: reqVal_p,
            params:
            {
                mmcode: T1QueryForm.getForm().findField('P1').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    st_LOT_NO.removeAll();
                    var lot_no = data.etts;
                    if (lot_no.length > 0) {
                        st_LOT_NO.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < lot_no.length; i++) {
                            st_LOT_NO.add({ VALUE: lot_no[i].VALUE, TEXT: lot_no[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setEXP_DATE() {
        Ext.Ajax.request({
            url: '/api/AA0076/Getst_EXP_DATE',
            method: reqVal_p,
            params:
            {
                mmcode: T1QueryForm.getForm().findField('P1').getValue(),
                lot_no: T1QueryForm.getForm().findField('P2').getValue(),
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    st_EXP_DATE.removeAll();
                    var exp_date = data.etts;
                    if (exp_date.length > 0) {
                        st_EXP_DATE.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < exp_date.length; i++) {
                            st_EXP_DATE.add({ VALUE: exp_date[i].VALUE, TEXT: exp_date[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    

    var T1QueryForm = Ext.widget({
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
            labelWidth: 60
        },
        defaultType: 'textfield',
        items: [
            {
                //xtype: 'textfield',
                //fieldLabel: '庫房代碼',
                //name: 'P0',
                //id: 'P0',
                //enforceMaxLength: true, // 限制可輸入最大長度
                //maxLength: 8, // 可輸入最大長度為100
                //padding: '0 4 0 4',
                //allowBlank: false,
                //fieldCls: 'required',

                xtype: 'combo',
                store: whnoQueryStore,
                name: 'P0',
                id: 'P0',
                fieldLabel: '庫房代碼',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                anyMatch: true,
                //allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                //fieldCls: 'required',
                multiSelect: false,
                blankText: "請選擇庫房代碼",
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {
                        
                        var wh_no = newValue.data.VALUE;
                        setMmcodeCombo(wh_no);

                        var f = T1QueryForm.getForm();
                        var u = f.findField('P1');
                        u.clearValue();
                        u.clearInvalid();
                    }
                }
            },
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '院內碼',
            //    name: 'P1',
            //    id: 'P1',
            //    enforceMaxLength: true,
            //    maxLength: 13,
            //    padding: '0 4 0 4',
            //},
            T1QuryMMCode
            ,
            {
                xtype: 'combo',
                fieldLabel: '批號',
                store: st_LOT_NO,
                name: 'P2',
                id: 'P2',
                labelWidth: 70,
                width: 220,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                listConfig:
                {
                    width: 180
                },
                matchFieldWidth: false,
                listeners: {
                    select: function (oldValue, newValue, eOpts) {
                        //選取下拉項目時，顯示回傳值
                        if (T1QueryForm.getForm().findField('P1').isValid() && T1QueryForm.getForm().findField('P2').isValid()) {

                            setEXP_DATE();

                            var f = T1QueryForm.getForm();
                            var u = f.findField('P3');
                            u.clearValue();
                            u.clearInvalid();
                        } else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>院內碼,批號</span>需填');
                        }
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '效期',
                store: st_EXP_DATE,
                name: 'P3',
                id: 'P3',
                labelWidth: 70,
                width: 220,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                listConfig:
                {
                    width: 180
                },
                matchFieldWidth: false,
            },
            {
                xtype: 'combo',
                store: storehourse,
                fieldLabel: '庫存零/非零',
                name: 'P5',
                id: 'P5',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                autoSelect: true,
                anyMatch: true,
                multiSelect: false,
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                matchFieldWidth: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                labelWidth: 50,
                width: 170
            },
            {
                xtype: 'combo',
                store: e_vaccine,
                fieldLabel: '疫苗/非疫苗',
                name: 'P6',
                id: 'P6',
                displayField: 'TEXT',
                valueField: 'VALUE',
                queryMode: 'local',
                autoSelect: true,
                anyMatch: true,
                multiSelect: false,
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                matchFieldWidth: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                labelWidth: 50,
                width: 170
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                var f = T1QueryForm.getForm();
                if (f.isValid()) {
                    T1Load(true);
                } else {
                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                }
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiWexpinv', { // /Scripts/app/store/MiWexpinv.js
        listeners: {
            beforeload: function (store, options) {
                var np = { //效期改成下拉式
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue(),
                    p6: T1QueryForm.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load(clearMsg) {
        T1Tool.moveFirst();
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }


    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AA0076/Create';
                    setFormT1('I', '新增');
                    TATabs.setActiveTab('Form');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0076/Update';
                    setFormT1("U", '修改');
                    TATabs.setActiveTab('Form');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0076/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                itemId: 'export', text: '匯出', //T1Query
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '批號效期維護' });

                    if (T1QueryForm.getForm().findField('P0').isValid()) {
                        p.push({ name: 'p0', value: T1QueryForm.getForm().findField('P0').getValue() });

                        T1GetExcel = '../../../api/AA0076/Excel';
                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    } else {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                        msglabel('訊息區:');
                    }

                }
            }, {
                xtype: 'filefield',
                name: 'filefield',
                displayfield: '上傳檔案',
                buttonText: '選擇檔案',
                margin:5,
                width: 200,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            T1Query.getForm().findField('filefield').fileInputEl.dom.value = '';
                            msglabel("訊息區:");
                        } else {
                            Ext.getCmp('check').setDisabled(false);

                            msglabel("已選擇檔案");
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'check',
                name: 'check',
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var formData = new FormData();
                    formData.append("file", file);
                    var ajaxRequest = $.ajax({
                        type: "POST",
                        url: "/api/AA0076/CheckExcel",
                        data: formData,
                        processData: false,
                        //必須false才會自動加上正確的Content-Type
                        contentType: false,
                        success: function (data, textStatus, jqXHR) {
                            myMask.hide();
                            if (!data.success) {
                                T1Store.removeAll();
                                Ext.MessageBox.alert("提示", data.msg);
                                msglabel("訊息區:");
                            }
                            else {
                                Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                msglabel("訊息區:處理完成!");
                                T1Store.loadData(data.etts, false);
                            }

                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }
        ]
    });
    function setFormT1(x, t) {
        
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);// + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MiWexpinv'); // /Scripts/app/model/MiWexpinv.js
            T1Form.loadRecord(r);
            u = f.findField("MMCODE");
            u.setReadOnly(false);
            u.clearInvalid();
            u = f.findField("MMNAME_C");
            u.setValue("");
            u = f.findField("MMNAME_E");
            u.setValue("");
            u = f.findField("WH_NO");
            u.setReadOnly(false);
            u.clearInvalid();
            u = f.findField('INV_QTY');
            u.setValue(0);

            setCmpShowCondition(true, false, true, false, false);
        }
        else {
            u = f.findField("MMCODE");
            u.clearInvalid();
            u = f.findField("WH_NO");
            u.clearInvalid();
            f.findField("ORI_EXP_DATE").setValue(T1LastRec.data.EXP_DATE);


            var wh_no = f.findField("WH_NO").getValue();
            setMmcodeCombo(wh_no);
            var promise = mmcodeComboPromise(wh_no);
            u = f.findField('INV_QTY');

            setCmpShowCondition(false, true, false, false, true);
        }

        f.findField('x').setValue(x);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').clearInvalid();
        f.findField('LOT_NO').clearInvalid();
        f.findField('INV_QTY').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
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
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        },
        {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 180
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 100
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 80,

        }, {
            xtype: 'numbercolumn',
            text: "庫存數量",
            dataIndex: 'INV_QTY',
            width: 80,
            format: '0.00',
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "檢核結果",
            dataIndex: 'STATUS_DISPLAY',
            width: 190
        }],

        viewConfig: {
            listeners: {
                selectionchange: function (model, records) {
                    
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    
                    if (T1LastRec) {
                        var wh_no = T1LastRec.data.WH_NO;
                        var promise = mmcodeComboPromise(wh_no);
                        promise.then(function (success) {
                            
                            var data = JSON.parse(success);
                            if (data.success) {
                                var mmcodes = data.etts;
                                mmcodeQueryStore.removeAll();
                                if (mmcodes.length > 0) {
                                    mmcodeQueryStore.add({ VALUE: '', TEXT: '' });
                                    for (var i = 0; i < mmcodes.length; i++) {
                                        mmcodeQueryStore.add({ VALUE: mmcodes[i].VALUE, TEXT: mmcodes[i].TEXT });
                                    }
                                }

                                setFormT1a();
                                TATabs.setActiveTab('Form');
                            }
                        }
                        )
                    }

                }, cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {

                    //得到庫房代碼 EX: 010000
                    var clickedDataIndex = view.panel.headerCt.getHeaderAtIndex(1).dataIndex;
                    var clickedColumnName = view.panel.headerCt.getHeaderAtIndex(1).text;
                    var clickedCellValue = record.get(clickedDataIndex);

                    //與目前的庫房比較,所以使用者之後要新增，則需要按下那筆才能修改
                    var exit = whnoQueryStore.findExact('VALUE', clickedCellValue);

                    if (exit >= 0) {
                        T1Grid.down('#edit').setDisabled(false);
                        T1Grid.down('#delete').setDisabled(false);
                    } else {
                        T1Grid.down('#edit').setDisabled(true);
                        T1Grid.down('#delete').setDisabled(true);
                    }
                }
            }
        }
    });
    function setFormT1a() {
        //

        //
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('WH_NO');
            u.clearInvalid();
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            u = f.findField('MMCODE');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('LOT_NO');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('EXP_DATE');
            u.clearInvalid();
            u.setReadOnly(true);

            u = f.findField('INV_QTY');
            u.clearInvalid();
            u.setReadOnly(true);
        }
        else {
            T1Form.getForm().reset();
        }
    }


    var T1AddMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0076/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        allowBlank: false, // 欄位為必填
        fieldCls: 'required',
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                wh_no: T1Form.getForm().findField('WH_NO').getValue(),  //p0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                if (T1Form.getForm().findField('P0').isValid()) {

                } else {
                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                }
            }
        },
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
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: 'right'
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },
        {
            fieldLabel: 'ORI_EXP_DATE',
            name: 'ORI_EXP_DATE',
            xtype: 'hidden'
        }, {
            xtype: 'combo',
            store: whnoQueryStore,
            fieldLabel: '庫房代碼',
            name: 'WH_NO',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            typeAhead: true,
            forceSelection: true,
            queryMode: 'local',
            triggerAction: 'all',
            fieldCls: 'required',
            multiSelect: false,
            blankText: "請選擇庫房代碼",
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            listeners: {
                select: function (oldValue, newValue, eOpts) {
                    
                    var wh_no = newValue.data.VALUE;
                    setMmcodeCombo(wh_no);
                    clearFormForWhno();
                }
            }
        },
        {
            xtype: 'displayfield',
            fieldLabel: '庫房代碼',
            name: 'WH_NAME_DISPLAY',
        },
            T1AddMMCode
         ,
        //{
        //    fieldLabel: '院內碼',
        //    name: 'MMCODE_TEXT',
        //    enforceMaxLength: true,
        //    maxLength: 20,
        //    readOnly: true,
        //    fieldCls: 'required'
        //},
        //{
        //    fieldLabel: '院內碼',
        //    name: 'MMCODE',
        //    enforceMaxLength: true,
        //    maxLength: 20,
        //    readOnly: true,
        //    fieldCls: 'required',
        //    listeners: {
        //        blur: function (field, eve, eOpts) {
        //            if (field.getValue() != '')
        //                chkMMCODE(field.getValue());
        //        }
        //    }
        //},
        {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE_DISPLAY'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        },
        {
            fieldLabel: '批號',
            name: 'LOT_NO',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
            //}, {
            //    fieldLabel: '效期',
            //    name: 'EXP_DATE',
            //    enforceMaxLength: true,
            //    maxLength: 7,
            //    regexText: '請填入民國年月日',
            //    regex: /\d{7,7}/,
            //    readOnly: true,
            //    allowBlank: false,
            //    fieldCls: 'required'
            //}, {
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            name: 'EXP_DATE',
            //enforceMaxLength: true,
            //maxLength: 7,
            //regexText: '請填入民國年月日',
            //regex: /\d{7,7}/,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'numberfield',
            fieldLabel: '庫存數量',
            name: 'INV_QTY',
            enforceMaxLength: false,
            maxLength: 14,
            readOnly: true,
            hideTrigger: true
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
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
                }

            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function setMmcodeCombo(wh_no) {
        var promise = mmcodeComboPromise(wh_no);
        promise.then(function (success) {
            
            var data = JSON.parse(success);
            if (data.success) {
                var mmcodes = data.etts;
                mmcodeQueryStore.removeAll();
                if (mmcodes.length > 0) {
                    mmcodeQueryStore.add({ VALUE: '', TEXT: '' });
                    for (var i = 0; i < mmcodes.length; i++) {
                        mmcodeQueryStore.add({ VALUE: mmcodes[i].VALUE, TEXT: mmcodes[i].TEXT });
                    }
                }
                //setFormT1a();
            }
        }
        )
    }
    var clearFormForWhno = function () {
        var f = T1Form.getForm();
        var u = f.findField('MMCODE');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('EXP_DATE');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('LOT_NO');
        u.setValue("");
        u.clearInvalid();
        u = f.findField('INV_QTY');
        u.clearInvalid();
        u.setValue(0);
    }
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    //T1LastRec = false;
                    //console.log(T1LastRec);
                    setCmpShowCondition(true, false, true, true, false);

                    var query = T1QueryForm.getForm();
                    
                    query.findField("P0").setValue(f.findField('WH_NO').getValue());
                    query.findField("P1").setValue(f.findField('MMCODE').getValue());
                    query.findField("P2").setValue(f.findField('LOT_NO').getValue());
                    query.findField("P3").setValue(f.findField('EXP_DATE').getValue());
                    query.findField("P4").setValue(f.findField('EXP_DATE').getValue());

                    var x = f.findField('x').getValue();
                    if (x === "I") {
                        msglabel('訊息區:資料新增成功');
                    } else if (x === "U") {
                        msglabel('訊息區:資料修改成功');
                    } else {
                        msglabel('訊息區:資料刪除成功');
                    }


                    T1Load(false);
                    T1Cleanup();
                    TATabs.setActiveTab('Query');
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
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "numberfield" || fc.xtype == "combo") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('');
        setFormT1a();

        //修改,刪除關閉
        T1Grid.down('#edit').setDisabled(true);
        T1Grid.down('#delete').setDisabled(true);
        setCmpShowCondition(true, false, true, true, false);
    }
    function chkMMCODE(parMmcode) {
        
        var wh_no = T1Form.getForm().findField('WH_NO').getValue();
        if (wh_no == "") {
            Ext.Msg.alert('提醒', '請先選擇<span style=\'color:red\'>庫房代碼</span>');
            return
        }
        Ext.Ajax.request({
            url: GetMmdataByMmcode,
            method: reqVal_p,
            params: { mmcode: parMmcode, wh_no: wh_no },
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success == false) {
                    Ext.Msg.alert('訊息', data.msg);
                    T1Form.getForm().findField('MMCODE').setValue('');
                    T1Form.getForm().findField('MMNAME_C').setValue('');
                    T1Form.getForm().findField('MMNAME_E').setValue('');
                    return;
                }

                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T1Form.getForm().findField('MMNAME_C').setValue(tb_data[0].MMNAME_C);
                        T1Form.getForm().findField('MMNAME_E').setValue(tb_data[0].MMNAME_E);
                    }
                    else {
                        Ext.Msg.alert('訊息', '院內碼不存在,請重新輸入!');
                        T1Form.getForm().findField('MMCODE').setValue('');
                        T1Form.getForm().findField('MMNAME_C').setValue('');
                        T1Form.getForm().findField('MMNAME_E').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }


    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }
    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
    }

    function mmcodeComboPromise(wh_no) {
        var deferred = new Ext.Deferred();

        Ext.Ajax.request({
            url: MmcodeComboGet,
            method: reqVal_p,
            params: {
                WH_NO: wh_no,
                page: 0,
                limit: 20,
                p0:''
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

    function setCmpShowCondition(wh_no, wh_name_display, mmcode, mmcode_text, mmcode_display) {
        var f = T1Form.getForm();
        wh_no ? showComponent(f, "WH_NO") : hideComponent(f, "WH_NO");
        wh_name_display ? showComponent(f, "WH_NAME_DISPLAY") : hideComponent(f, "WH_NAME_DISPLAY");
        mmcode ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        mmcode_display ? showComponent(f, "MMCODE_DISPLAY") : hideComponent(f, "MMCODE_DISPLAY");
        //mmcode_text ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
    }

    //view 
    var TATabs = Ext.widget('tabpanel', {
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
            items: [T1QueryForm]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form]
        }]
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
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });

    setCmpShowCondition(true, false, true, true, false);



    T1QueryForm.getForm().findField('P0').focus();

    T1QueryForm.getForm().findField('P5').setValue('1');
    T1QueryForm.getForm().findField('P6').setValue('N');
    Ext.getCmp('check').setDisabled(true);

});
